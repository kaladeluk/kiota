using System.Text;
using System.Text.RegularExpressions;

namespace Kaladel.Kiota.NullabilityFix.TypeScript;

/// <summary>
/// <para>
/// Rewrites a Kiota-generated TypeScript <c>models/index.ts</c> file so that properties the
/// source OpenAPI schema marks as "required" are emitted as required TypeScript properties
/// (dropping the <c>?</c>), and drops the <c>| null</c> union member unless the schema also
/// marks the property as nullable.
/// </para>
/// <para>
/// This operates on Kiota's generated output as plain text rather than via a full TypeScript
/// parser. That's deliberate: the input is fully machine-generated, highly regular boilerplate
/// (one interface per schema, one property per line, consistent formatting) rather than
/// hand-written code, which makes a targeted line-based rewrite both safe and far simpler than
/// pulling in a full TS AST toolchain.
/// </para>
/// </summary>
public static partial class ModelFileRewriter
{
    // Matches the start of a generated interface, e.g.:
    //     export interface StatusGetResponse extends AdditionalDataHolder, Parsable {
    [GeneratedRegex(@"^export interface (?<name>[A-Za-z0-9_]+)\b.*\{\s*$", RegexOptions.Compiled)]
    private static partial Regex InterfaceStartPattern();

    // Matches a generated property declaration, e.g.:
    //     hasAccounts?: boolean | null;
    //     currency?: StaticCurrency | null;
    [GeneratedRegex(@"^(?<indent>\s*)(?<name>[A-Za-z0-9_]+)\?:\s*(?<type>.+?)\s*\|\s*null;\s*$", RegexOptions.Compiled)]
    private static partial Regex PropertyPattern();

    public static string Rewrite(
        string fileContents,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, PropertyNullability>> schemaMap,
        out int interfacesSeen,
        out int propertiesTightened)
    {
        var endsWithNewline = fileContents.EndsWith('\n');
        var lines = fileContents.Replace("\r\n", "\n").Split('\n');
        var output = new StringBuilder(fileContents.Length);

        string? currentInterfaceName = null;
        IReadOnlyDictionary<string, PropertyNullability>? currentPropertyMap = null;
        interfacesSeen = 0;
        propertiesTightened = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var isLastLine = i == lines.Length - 1;

            var interfaceMatch = InterfaceStartPattern().Match(line);
            if (interfaceMatch.Success)
            {
                currentInterfaceName = interfaceMatch.Groups["name"].Value;
                schemaMap.TryGetValue(currentInterfaceName, out currentPropertyMap);
                interfacesSeen++;

                AppendLine(output, line, isLastLine);
                continue;
            }

            if (currentInterfaceName is not null && line.Length == 1 && line[0] == '}')
            {
                currentInterfaceName = null;
                currentPropertyMap = null;

                AppendLine(output, line, isLastLine);
                continue;
            }

            if (currentPropertyMap is not null)
            {
                var propertyMatch = PropertyPattern().Match(line);
                if (propertyMatch.Success)
                {
                    var propertyName = propertyMatch.Groups["name"].Value;

                    if (currentPropertyMap.TryGetValue(propertyName, out var nullability) && nullability.IsRequired)
                    {
                        var indent = propertyMatch.Groups["indent"].Value;
                        var type = propertyMatch.Groups["type"].Value;

                        var rewritten = nullability.IsNullable
                            ? $"{indent}{propertyName}: {type} | null;"
                            : $"{indent}{propertyName}: {type};";

                        AppendLine(output, rewritten, isLastLine);
                        propertiesTightened++;
                        continue;
                    }
                }
            }

            AppendLine(output, line, isLastLine);
        }

        return output.ToString();

        void AppendLine(StringBuilder builder, string text, bool isLast)
        {
            builder.Append(text);
            if (!isLast || endsWithNewline)
            {
                builder.Append('\n');
            }
        }
    }
}
