# Kaladel.Kiota

A collection of small .NET tools/extensions for working around rough edges in [Kiota](https://github.com/microsoft/kiota)-generated API clients.

## Tools

| Tool | Package | Description |
| --- | --- | --- |
| [Kaladel.Kiota.NullabilityFix.TypeScript](Kaladel.Kiota.NullabilityFix.TypeScript/readme.md) | [![NuGet](https://img.shields.io/nuget/v/Kaladel.Kiota.NullabilityFix.TypeScript.svg)](https://www.nuget.org/packages/Kaladel.Kiota.NullabilityFix.TypeScript/) | Post-processes Kiota-generated TypeScript models so that `required` properties from the source OpenAPI document are emitted as required/non-nullable, instead of Kiota's default of making every property optional and nullable. |

See each tool's own README (linked above) for installation and usage instructions specific to that tool.

## Building

Open [Kaladel.Kiota.slnx](Kaladel.Kiota.slnx) in Visual Studio, or build from the command line:

```shell
dotnet build
dotnet test
```

## Contributing

Issues and pull requests are welcome. These are small, focused tools — before proposing new functionality, please open an issue to discuss the change first.

## License

Licensed under the [MIT License](LICENSE).
