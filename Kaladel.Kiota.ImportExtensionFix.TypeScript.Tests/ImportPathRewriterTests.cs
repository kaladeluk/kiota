namespace Kaladel.Kiota.ImportExtensionFix.TypeScript.Tests;

[TestClass]
public class ImportPathRewriterTests
{
    [TestMethod]
    public void Rewrite_RelativeIndexJsImport_StripsIndexJsSuffix()
    {
        const string input =
            "import { DebtforecastsRequestBuilderNavigationMetadata, type DebtforecastsRequestBuilder } from './debtforecasts/index.js';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(1, importsFixed);
        Assert.Contains("from './debtforecasts';", result);
        Assert.DoesNotContain("index.js", result);
    }

    [TestMethod]
    public void Rewrite_RelativeJsImport_StripsJsSuffix()
    {
        const string input = "import { Foo } from './models/foo.js';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(1, importsFixed);
        Assert.Contains("from './models/foo';", result);
    }

    [TestMethod]
    public void Rewrite_ParentRelativeImport_IsRewritten()
    {
        const string input = "import { Foo } from '../models/foo.js';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(1, importsFixed);
        Assert.Contains("from '../models/foo';", result);
    }

    [TestMethod]
    public void Rewrite_ExportFromStatement_IsRewritten()
    {
        const string input = "export { Bar } from \"./bar/index.js\";\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(1, importsFixed);
        Assert.Contains("export { Bar } from \"./bar\";", result);
    }

    [TestMethod]
    public void Rewrite_BarePackageImport_IsLeftUntouched()
    {
        const string input = "import { Parsable } from '@microsoft/kiota-abstractions';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(0, importsFixed);
        Assert.Contains("from '@microsoft/kiota-abstractions';", result);
    }

    [TestMethod]
    public void Rewrite_RelativeImportWithoutJsExtension_IsLeftUntouched()
    {
        const string input = "import { Foo } from './foo';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(0, importsFixed);
        Assert.Contains("from './foo';", result);
    }

    [TestMethod]
    public void Rewrite_MultipleImportsInFile_RewritesEachIndependently()
    {
        const string input =
            "import { Foo } from './foo/index.js';\n" +
            "import { Bar } from './bar.js';\n" +
            "import { Parsable } from '@microsoft/kiota-abstractions';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(2, importsFixed);
        Assert.Contains("from './foo';", result);
        Assert.Contains("from './bar';", result);
        Assert.Contains("from '@microsoft/kiota-abstractions';", result);
    }

    [TestMethod]
    public void Rewrite_MultilineImportStatement_IsRewritten()
    {
        const string input =
            "import {\n" +
            "    Foo,\n" +
            "    Bar,\n" +
            "} from './foo/index.js';\n";

        var result = ImportPathRewriter.Rewrite(input, out var importsFixed);

        Assert.AreEqual(1, importsFixed);
        Assert.Contains("from './foo';", result);
    }
}
