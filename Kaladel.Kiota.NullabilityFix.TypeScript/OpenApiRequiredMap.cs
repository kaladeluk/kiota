using Microsoft.OpenApi;

namespace Kaladel.Kiota.NullabilityFix.TypeScript;

/// <summary>
/// Describes how a single schema property should be emitted in the fixed-up TypeScript model:
/// whether it must always be present (<see cref="IsRequired"/>) and whether it may still be
/// <c>null</c> even when present (<see cref="IsNullable"/>) - these are orthogonal concepts in
/// OpenAPI 3.1 (a property can be required yet nullable, e.g. <c>type: ["string", "null"]</c>).
/// </summary>
public readonly record struct PropertyNullability(bool IsRequired, bool IsNullable);

/// <summary>
/// Loads an OpenAPI document and builds a lookup of schema name -> property name ->
/// <see cref="PropertyNullability"/>, used to drive the TypeScript model rewrite.
/// </summary>
public static class OpenApiRequiredMap
{
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, PropertyNullability>> Load(string openApiFilePath)
    {
        using var stream = File.OpenRead(openApiFilePath);
        var readResult = OpenApiDocument.LoadAsync(stream, format: "json").GetAwaiter().GetResult();

        if (readResult.Document is null)
        {
            var errors = string.Join(Environment.NewLine, readResult.Diagnostic?.Errors.Select(e => e.Message) ?? []);
            throw new InvalidOperationException($"Failed to parse OpenAPI document '{openApiFilePath}':{Environment.NewLine}{errors}");
        }

        var document = readResult.Document;
        var map = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>(StringComparer.OrdinalIgnoreCase);

        if (document.Components?.Schemas is null)
        {
            return map;
        }

        foreach (var (schemaName, schema) in document.Components.Schemas)
        {
            map[schemaName] = BuildPropertyMap(schema);
        }

        return map;
    }

    private static Dictionary<string, PropertyNullability> BuildPropertyMap(IOpenApiSchema schema)
    {
        var propertyMap = new Dictionary<string, PropertyNullability>(StringComparer.OrdinalIgnoreCase);

        if (schema.Properties is null)
        {
            return propertyMap;
        }

        foreach (var (propertyName, propertySchema) in schema.Properties)
        {
            var isRequired = schema.Required?.Contains(propertyName) == true;
            var isNullable = IsNullableSchema(propertySchema);

            propertyMap[propertyName] = new PropertyNullability(isRequired, isNullable);
        }

        return propertyMap;
    }

    /// <summary>
    /// Determines nullability per OpenAPI 3.1 semantics, where a nullable property is expressed
    /// as a <c>type</c> flags value including <see cref="JsonSchemaType.Null"/> (e.g. a schema
    /// declared as <c>type: ["string", "null"]</c>), rather than the OpenAPI 3.0-style
    /// <c>nullable: true</c> flag.
    /// </summary>
    private static bool IsNullableSchema(IOpenApiSchema schema)
    {
        return schema.Type?.HasFlag(JsonSchemaType.Null) == true;
    }
}
