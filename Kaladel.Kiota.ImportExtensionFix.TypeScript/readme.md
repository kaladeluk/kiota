# Kaladel.Kiota.ImportExtensionFix.TypeScript

[![NuGet](https://img.shields.io/nuget/v/Kaladel.Kiota.ImportExtensionFix.TypeScript.svg)](https://www.nuget.org/packages/Kaladel.Kiota.ImportExtensionFix.TypeScript/)

A small .NET tool that post-processes [Kiota](https://github.com/microsoft/kiota)-generated TypeScript API clients to strip unnecessary `.js` extensions (and redundant `/index` segments) from relative `import`/`export` specifiers.

## The problem

Kiota's TypeScript generator emits relative imports with an explicit `.js` extension, even though the generated files are `.ts` sources, e.g.:

```ts
import { DebtforecastsRequestBuilderNavigationMetadata, type DebtforecastsRequestBuilder } from './debtforecasts/index.js';
```

This is [intentional and considered correct](https://github.com/microsoft/kiota-typescript/issues/1644) by the Kiota team, since ESM module resolution requires the extension of the file as it will exist *after* compilation (`.js`), not the source extension (`.ts`). `tsc` itself has no issue with this. However, in practice this confuses type resolution in some editors/IDEs when navigating or hovering over generated types, since there's no literal `.js` file in the source tree for the IDE to resolve against. The trailing `/index` segment is also redundant in every project this tool has been used in, since directory-index resolution makes it unnecessary.

This tool rewrites relative import/export specifiers in place, removing both:

```ts
import { DebtforecastsRequestBuilderNavigationMetadata, type DebtforecastsRequestBuilder } from './debtforecasts';
```

Only *relative* specifiers (starting with `./` or `../`) are rewritten — bare package specifiers (e.g. `@microsoft/kiota-abstractions`) are left untouched, since those genuinely resolve via `node_modules` and must keep their extension/format as published.

## Installation

Install as a [.NET global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools):

```shell
dotnet tool install --global Kaladel.Kiota.ImportExtensionFix.TypeScript
```

This makes the `kiota-importfix-ts` command available on your `PATH`. You can also install it as a local tool scoped to a repository (via a [tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools)):

```shell
dotnet new tool-manifest # if you don't already have one
dotnet tool install Kaladel.Kiota.ImportExtensionFix.TypeScript
```

## Usage

Run the tool after generating (or regenerating) your Kiota TypeScript client, pointing it at the directory Kiota generated into:

```shell
kiota-importfix-ts --kiota-output ./src/generated
```

| Option | Required | Description |
| --- | --- | --- |
| `--kiota-output` | Yes | Path to the directory Kiota generated the TypeScript client into. All `.ts` files under this directory (recursively, excluding `node_modules`) are scanned. |

The tool rewrites matching files in place and prints a short summary of how many imports/exports were rewritten and across how many files.

### Typical workflow

Add a step after your existing `kiota generate` invocation (e.g. in an npm script, Makefile, or CI pipeline):

```shell
kiota generate --openapi ./openapi.json --output ./src/generated --language typescript
kiota-importfix-ts --kiota-output ./src/generated
```

Re-run after every regeneration, since the rewrite is applied directly to Kiota's output and is not preserved across regeneration. This tool can be combined with [Kaladel.Kiota.NullabilityFix.TypeScript](../Kaladel.Kiota.NullabilityFix.TypeScript/readme.md) in the same pipeline, in any order.

## Example

Kiota generates:

```ts
import { DebtforecastsRequestBuilderNavigationMetadata, type DebtforecastsRequestBuilder } from './debtforecasts/index.js';
import { parseNode } from './serialization.js';
import { Parsable } from '@microsoft/kiota-abstractions';
```

After running this tool:

```ts
import { DebtforecastsRequestBuilderNavigationMetadata, type DebtforecastsRequestBuilder } from './debtforecasts';
import { parseNode } from './serialization';
import { Parsable } from '@microsoft/kiota-abstractions';
```

## Limitations

- Only rewrites static `import ... from '...'` and `export ... from '...'` statements; dynamic `import('...')` expressions are not rewritten (Kiota does not currently generate these).
- Only relative specifiers are rewritten; bare package imports are always left untouched.
- Operates as a targeted text rewrite rather than a full TypeScript parser/AST transform, which is deliberate given the fully machine-generated, highly regular nature of Kiota's output.

## Contributing

Issues and pull requests are welcome. This is a small, focused tool — before proposing new functionality, please open an issue to discuss the change first.

## License

Licensed under the [MIT License](../LICENSE).
