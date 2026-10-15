# Kaladel.Kiota.NullabilityFix.TypeScript

[![NuGet](https://img.shields.io/nuget/v/Kaladel.Kiota.NullabilityFix.TypeScript.svg)](https://www.nuget.org/packages/Kaladel.Kiota.NullabilityFix.TypeScript/)

A small .NET tool that post-processes [Kiota](https://github.com/microsoft/kiota)-generated TypeScript API clients so that model properties correctly reflect the `required` constraint from the source OpenAPI document.

## The problem

Kiota deliberately generates every TypeScript model property as optional and nullable (`property?: Type | null;`), regardless of whether the property is listed under `required` in the OpenAPI schema. This is a [known, intentional limitation](https://github.com/microsoft/kiota/issues) of Kiota's TypeScript generator, but it means consumers of the generated client lose a significant amount of compile-time safety: there's no way for TypeScript to tell you that a property is guaranteed to be present on a response, or that it must be supplied on a request.

This tool closes that gap. After Kiota generates (or regenerates) your client, run this tool against the generated `models/index.ts` file and the OpenAPI document that produced it, and it will:

- Drop the `?` optional marker from any property listed in the schema's `required` array.
- Drop the `| null` union member from required properties, *unless* the schema also marks the property as nullable (OpenAPI 3.1 `type: ["string", "null"]`-style nullability), in which case it is retained — a property can be required *and* nullable at the same time.
- Leave all other (non-required) properties untouched.

The tool operates as a targeted, line-based text rewrite of Kiota's generated output rather than a full TypeScript parser/AST transform. This is deliberate: the input is fully machine-generated, highly regular boilerplate (one interface per schema, one property per line, consistent formatting), which makes this approach both safe and far simpler than depending on a full TypeScript toolchain.

## Installation

Install as a [.NET global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools):

```shell
dotnet tool install --global Kaladel.Kiota.NullabilityFix.TypeScript
```

This makes the `kiota-nullfix-ts` command available on your `PATH`. You can also install it as a local tool scoped to a repository (via a [tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools)):

```shell
dotnet new tool-manifest # if you don't already have one
dotnet tool install Kaladel.Kiota.NullabilityFix.TypeScript
```

## Usage

Run the tool after generating (or regenerating) your Kiota TypeScript client, pointing it at the OpenAPI document used to generate the client and the directory Kiota generated into:

```shell
kiota-nullfix-ts --openapi ./openapi.json --kiota-output ./src/generated
```

| Option | Required | Description |
| --- | --- | --- |
| `--openapi` | Yes | Path to the OpenAPI document that was used to generate the Kiota TypeScript client. |
| `--kiota-output` | Yes | Path to the directory Kiota generated the TypeScript client into (the directory containing `models/index.ts`). |

The tool rewrites `models/index.ts` in place and prints a short summary of how many interfaces were scanned and how many properties were tightened.

### Typical workflow

Add a step after your existing `kiota generate` invocation (e.g. in an npm script, Makefile, or CI pipeline):

```shell
kiota generate --openapi ./openapi.json --output ./src/generated --language typescript
kiota-nullfix-ts --openapi ./openapi.json --kiota-output ./src/generated
```

Re-run both commands whenever the OpenAPI document or generated client changes, since the rewrite is applied directly to Kiota's output and is not preserved across regeneration.

## Example

Given an OpenAPI schema where `hasAccounts` is required and non-nullable, and `cycleFrequency` is required but nullable:

```jsonc
{
  "type": "object",
  "required": ["hasAccounts", "cycleFrequency"],
  "properties": {
    "hasAccounts": { "type": "boolean" },
    "cycleFrequency": { "type": ["string", "null"], "enum": ["monthly", "annually"] }
  }
}
```

Kiota generates:

```ts
export interface StatusGetResponse extends AdditionalDataHolder, Parsable {
    hasAccounts?: boolean | null;
    cycleFrequency?: FrequencyType | null;
}
```

After running this tool:

```ts
export interface StatusGetResponse extends AdditionalDataHolder, Parsable {
    hasAccounts: boolean;
    cycleFrequency: FrequencyType | null;
}
```

## Limitations

- Only targets the generated `models/index.ts` file; it does not modify request builders or other generated files.
- Relies on Kiota's current, highly regular TypeScript output format. Significant changes to Kiota's generator output format in future versions may require updates to this tool.
- Schema names are matched by the OpenAPI component schema name, so this will not help with inline (non-referenced) schemas that Kiota names based on request/response context, if those names diverge from the component schema name.

## Contributing

Issues and pull requests are welcome. This is a small, focused tool — before proposing new functionality, please open an issue to discuss the change first.

## License

Licensed under the [MIT License](../LICENSE).
