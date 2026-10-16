using System.CommandLine;
using Kaladel.Kiota.ImportExtensionFix.TypeScript;

var kiotaOutputOption = new Option<DirectoryInfo>("--kiota-output")
{
    Description = "Path to the directory Kiota generated the TypeScript client into. All '.ts' files in this directory (recursively) are scanned.",
    Required = true,
};

var rootCommand = new RootCommand("Strips unnecessary '.js' extensions (and redundant '/index' segments) from relative import/export specifiers in Kiota-generated TypeScript files.")
{
    kiotaOutputOption,
};

rootCommand.SetAction(parseResult =>
{
    var kiotaOutputDirectory = parseResult.GetValue(kiotaOutputOption)!;

    if (!kiotaOutputDirectory.Exists)
    {
        Console.Error.WriteLine($"Could not find directory '{kiotaOutputDirectory.FullName}'.");
        return 1;
    }

    var typeScriptFiles = Directory.EnumerateFiles(kiotaOutputDirectory.FullName, "*.ts", SearchOption.AllDirectories)
        .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains("node_modules"))
        .ToList();

    Console.WriteLine($"Scanning {typeScriptFiles.Count} TypeScript file(s) under: {kiotaOutputDirectory.FullName}");

    var filesChanged = 0;
    var totalImportsFixed = 0;

    foreach (var filePath in typeScriptFiles)
    {
        var originalContents = File.ReadAllText(filePath);

        var rewritten = ImportPathRewriter.Rewrite(originalContents, out var importsFixed);

        if (importsFixed == 0)
        {
            continue;
        }

        File.WriteAllText(filePath, rewritten);

        filesChanged++;
        totalImportsFixed += importsFixed;
    }

    Console.WriteLine($"Done. Rewrote {totalImportsFixed} import(s)/export(s) across {filesChanged} file(s).");
    return 0;
});

return rootCommand.Parse(args).Invoke();
