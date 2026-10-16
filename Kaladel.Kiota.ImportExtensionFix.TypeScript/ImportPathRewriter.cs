using System.Text.RegularExpressions;

namespace Kaladel.Kiota.ImportExtensionFix.TypeScript;

/// <summary>
/// <para>
/// Rewrites a Kiota-generated TypeScript file so that relative ESM <c>import</c>/<c>export</c>
/// specifiers drop the unnecessary <c>.js</c> extension (and, where present, the redundant
/// trailing <c>/index</c> segment) that Kiota's TypeScript generator adds, e.g.:
/// </para>
/// <code>
/// import { Foo } from './foo/index.js';   -&gt;   import { Foo } from './foo';
/// import { Bar } from './bar.js';         -&gt;   import { Bar } from './bar';
/// </code>
/// <para>
/// Kiota's generator intentionally includes the <c>.js</c> extension because it is required by
/// the ESM module resolution specification when the emitted JavaScript is actually executed.
/// However, since the generated files are <c>.ts</c> sources consumed directly by a TypeScript
/// project (rather than hand-written ESM JavaScript), the extension is unnecessary and some
/// editor/IDE tooling struggles to resolve types through it.
/// </para>
/// <para>
/// Only relative specifiers (starting with <c>./</c> or <c>../</c>) are rewritten; bare package
/// specifiers (e.g. <c>@microsoft/kiota-abstractions</c>) are left untouched.
/// </para>
/// </summary>
public static partial class ImportPathRewriter
{
    // Matches the specifier string of a static `import ... from '...'` or `export ... from '...'`
    // statement, e.g.:
    //     import { Foo } from './foo/index.js';
    //     export { Bar } from "../bar.js";
    // Captures the quote character used and the path inside it, so it can be reconstructed.
    [GeneratedRegex(
        @"(?<keyword>\bimport\b|\bexport\b)(?<middle>(?:(?!from\s*['""]).)*?from\s*)(?<quote>['""])(?<path>\.\.?/[^'""]*?)\k<quote>",
        RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ImportFromSpecifierPattern();

    public static string Rewrite(string fileContents, out int importsFixed)
    {
        var fixedCount = 0;

        var result = ImportFromSpecifierPattern().Replace(fileContents, match =>
        {
            var path = match.Groups["path"].Value;
            var rewrittenPath = RewritePath(path);

            if (rewrittenPath == path)
            {
                return match.Value;
            }

            fixedCount++;

            var quote = match.Groups["quote"].Value;
            return $"{match.Groups["keyword"].Value}{match.Groups["middle"].Value}{quote}{rewrittenPath}{quote}";
        });

        importsFixed = fixedCount;
        return result;
    }

    private static string RewritePath(string path)
    {
        const string indexJsSuffix = "/index.js";
        const string jsSuffix = ".js";

        if (path.EndsWith(indexJsSuffix, StringComparison.Ordinal))
        {
            return path[..^indexJsSuffix.Length];
        }

        if (path.EndsWith(jsSuffix, StringComparison.Ordinal))
        {
            return path[..^jsSuffix.Length];
        }

        return path;
    }
}
