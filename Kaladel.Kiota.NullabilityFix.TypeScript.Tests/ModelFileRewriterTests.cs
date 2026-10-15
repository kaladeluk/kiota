namespace Kaladel.Kiota.NullabilityFix.TypeScript.Tests;

[TestClass]
public class ModelFileRewriterTests
{
    [TestMethod]
    public void Rewrite_RequiredNonNullableProperty_DropsOptionalAndNullMarkers()
    {
        const string input =
            "export interface StatusGetResponse extends AdditionalDataHolder, Parsable {\n" +
            "    /**\n" +
            "     * The hasAccounts property\n" +
            "     */\n" +
            "    hasAccounts?: boolean | null;\n" +
            "}\n";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>
        {
            ["StatusGetResponse"] = new Dictionary<string, PropertyNullability>
            {
                ["hasAccounts"] = new PropertyNullability(IsRequired: true, IsNullable: false),
            },
        };

        var result = ModelFileRewriter.Rewrite(input, schemaMap, out var interfacesSeen, out var propertiesTightened);

        Assert.AreEqual(1, interfacesSeen);
        Assert.AreEqual(1, propertiesTightened);
        Assert.Contains("    hasAccounts: boolean;", result);
        Assert.DoesNotContain("hasAccounts?:", result);
    }

    [TestMethod]
    public void Rewrite_RequiredNullableProperty_DropsOptionalButKeepsNull()
    {
        const string input =
            "export interface AccountGetResponse extends AdditionalDataHolder, Parsable {\n" +
            "    cycleFrequency?: FrequencyType | null;\n" +
            "}\n";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>
        {
            ["AccountGetResponse"] = new Dictionary<string, PropertyNullability>
            {
                ["cycleFrequency"] = new PropertyNullability(IsRequired: true, IsNullable: true),
            },
        };

        var result = ModelFileRewriter.Rewrite(input, schemaMap, out _, out var propertiesTightened);

        Assert.AreEqual(1, propertiesTightened);
        Assert.Contains("    cycleFrequency: FrequencyType | null;", result);
    }

    [TestMethod]
    public void Rewrite_OptionalProperty_IsLeftUntouched()
    {
        const string input =
            "export interface MortgagePatchRequest extends AdditionalDataHolder, Parsable {\n" +
            "    notes?: string | null;\n" +
            "}\n";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>
        {
            ["MortgagePatchRequest"] = new Dictionary<string, PropertyNullability>
            {
                ["notes"] = new PropertyNullability(IsRequired: false, IsNullable: true),
            },
        };

        var result = ModelFileRewriter.Rewrite(input, schemaMap, out _, out var propertiesTightened);

        Assert.AreEqual(0, propertiesTightened);
        Assert.Contains("    notes?: string | null;", result);
    }

    [TestMethod]
    public void Rewrite_SamePropertyNameAcrossInterfaces_UsesOwnInterfaceSchema()
    {
        const string input =
            "export interface ScheduledTransactionGetResponse extends AdditionalDataHolder, Parsable {\n" +
            "    frequency?: FrequencyType | null;\n" +
            "}\n" +
            "export interface ScheduledTransactionPatchRequest extends AdditionalDataHolder, Parsable {\n" +
            "    frequency?: FrequencyType | null;\n" +
            "}\n";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>
        {
            ["ScheduledTransactionGetResponse"] = new Dictionary<string, PropertyNullability>
            {
                ["frequency"] = new PropertyNullability(IsRequired: true, IsNullable: false),
            },
            ["ScheduledTransactionPatchRequest"] = new Dictionary<string, PropertyNullability>
            {
                ["frequency"] = new PropertyNullability(IsRequired: false, IsNullable: true),
            },
        };

        var result = ModelFileRewriter.Rewrite(input, schemaMap, out var interfacesSeen, out var propertiesTightened);

        Assert.AreEqual(2, interfacesSeen);
        Assert.AreEqual(1, propertiesTightened);

        var lines = result.Split('\n');
        Assert.AreEqual("    frequency: FrequencyType;", lines[1]);
        Assert.AreEqual("    frequency?: FrequencyType | null;", lines[4]);
    }

    [TestMethod]
    public void Rewrite_SchemaNotInMap_LeavesInterfaceUntouched()
    {
        const string input =
            "export interface UnknownResponse extends AdditionalDataHolder, Parsable {\n" +
            "    foo?: string | null;\n" +
            "}\n";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>();

        var result = ModelFileRewriter.Rewrite(input, schemaMap, out var interfacesSeen, out var propertiesTightened);

        Assert.AreEqual(1, interfacesSeen);
        Assert.AreEqual(0, propertiesTightened);
        Assert.Contains("    foo?: string | null;", result);
    }

    [TestMethod]
    public void Rewrite_PreservesTrailingNewlineBehaviour()
    {
        const string inputWithTrailingNewline = "export interface Foo extends AdditionalDataHolder, Parsable {\n}\n";
        const string inputWithoutTrailingNewline = "export interface Foo extends AdditionalDataHolder, Parsable {\n}";

        var schemaMap = new Dictionary<string, IReadOnlyDictionary<string, PropertyNullability>>();

        var resultWith = ModelFileRewriter.Rewrite(inputWithTrailingNewline, schemaMap, out _, out _);
        var resultWithout = ModelFileRewriter.Rewrite(inputWithoutTrailingNewline, schemaMap, out _, out _);

        Assert.IsTrue(resultWith.EndsWith('\n'));
        Assert.IsFalse(resultWithout.EndsWith('\n'));
    }
}
