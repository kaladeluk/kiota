# Kaladel.Kiota

A collection of small .NET tools/extensions for working around rough edges in [Kiota](https://github.com/microsoft/kiota)-generated API clients.

## Tools

| Tool | Package | Description |
| --- | --- | --- |
| [Kaladel.Kiota.ImportExtensionFix.TypeScript](Kaladel.Kiota.ImportExtensionFix.TypeScript/readme.md) | [![NuGet](https://img.shields.io/nuget/v/Kaladel.Kiota.ImportExtensionFix.TypeScript.svg)](https://www.nuget.org/packages/Kaladel.Kiota.ImportExtensionFix.TypeScript/) | Post-processes Kiota-generated TypeScript clients to strip unnecessary `.js` extensions (and redundant `/index` segments) from relative import/export specifiers. |
| [Kaladel.Kiota.NullabilityFix.TypeScript](Kaladel.Kiota.NullabilityFix.TypeScript/readme.md) | [![NuGet](https://img.shields.io/nuget/v/Kaladel.Kiota.NullabilityFix.TypeScript.svg)](https://www.nuget.org/packages/Kaladel.Kiota.NullabilityFix.TypeScript/) | Post-processes Kiota-generated TypeScript models so that `required` properties from the source OpenAPI document are emitted as required/non-nullable, instead of Kiota's default of making every property optional and nullable. |

See each tool's own README (linked above) for installation and usage instructions specific to that tool.

## Building

Open [Kaladel.Kiota.slnx](Kaladel.Kiota.slnx) in Visual Studio, or build from the command line:

```shell
dotnet build
dotnet test
```

## Releasing

Each tool is published to NuGet.org independently via its own GitHub Actions workflow in [.github/workflows](.github/workflows), using NuGet.org's [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) (OIDC) rather than a stored API key. To cut a release for a tool:

1. Bump `<Version>` in the tool's `.csproj`.
2. Push a tag matching that tool's pattern (e.g. `nullabilityfix-ts-v0.1.0` for `Kaladel.Kiota.NullabilityFix.TypeScript`) — this triggers the matching workflow, which builds, tests, packs, pushes to NuGet.org, and creates a matching GitHub release.

Each tool's workflow must have a corresponding Trusted Publishing policy configured on NuGet.org (Account → Trusted Publishing) with the repository owner/name and the workflow file name (e.g. `publish-nullabilityfix-ts.yml`).

## Contributing

Issues and pull requests are welcome. These are small, focused tools — before proposing new functionality, please open an issue to discuss the change first.

## License

Licensed under the [MIT License](LICENSE).
