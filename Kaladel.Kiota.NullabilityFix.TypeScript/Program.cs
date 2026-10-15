using System.CommandLine;
using Kaladel.Kiota.NullabilityFix.TypeScript;

var openApiOption = new Option<FileInfo>("--openapi")
{
    Description = "Path to the OpenAPI document that was used to generate the Kiota TypeScript client.",
    Required = true,
};

var kiotaOutputOption = new Option<DirectoryInfo>("--kiota-output")
{
    Description = "Path to the directory Kiota generated the TypeScript client into (the directory containing 'models/index.ts').",
    Required = true,
};

var rootCommand = new RootCommand("Tightens Kiota-generated TypeScript model types to match 'required'/nullable information from the source OpenAPI document.")
{
    openApiOption,
    kiotaOutputOption,
};

rootCommand.SetAction(parseResult =>
{
    var openApiFile = parseResult.GetValue(openApiOption)!;
    var kiotaOutputDirectory = parseResult.GetValue(kiotaOutputOption)!;

    var modelsFilePath = Path.Combine(kiotaOutputDirectory.FullName, "models", "index.ts");

    if (!File.Exists(modelsFilePath))
    {
        Console.Error.WriteLine($"Could not find generated models file at '{modelsFilePath}'.");
        return 1;
    }

    Console.WriteLine($"Loading OpenAPI document: {openApiFile.FullName}");
    var schemaMap = OpenApiRequiredMap.Load(openApiFile.FullName);

    Console.WriteLine($"Rewriting: {modelsFilePath}");
    var originalContents = File.ReadAllText(modelsFilePath);

    var rewritten = ModelFileRewriter.Rewrite(originalContents, schemaMap, out var interfacesSeen, out var propertiesTightened);

    File.WriteAllText(modelsFilePath, rewritten);

    Console.WriteLine($"Done. Scanned {interfacesSeen} interfaces, tightened {propertiesTightened} properties.");
    return 0;
});

return rootCommand.Parse(args).Invoke();
