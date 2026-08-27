using CobolMutantForge.Infrastructure.Serialization;
using Xunit;

namespace CobolMutantForge.Tests.Serialization;

public class ZUnitConfigParserTests
{
    [Fact]
    public void Parse_KeyValueLines_ProducesConfigWithParameters()
    {
        var parser = new ZUnitConfigParser();
        const string content =
            "[ACCOUNT-TEST]\r\n" +
            "testContext = CICS-REGION\r\n" +
            "TIMEOUT = 30\r\n" +
            "RETRY = 3\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT-TEST", result.Config.Name);
        Assert.Equal("CICS-REGION", result.Config.TestContext);
        Assert.Equal("30", result.Config.Parameters["TIMEOUT"]);
        Assert.Equal("3", result.Config.Parameters["RETRY"]);
    }

    [Fact]
    public void Parse_Json_ProducesConfig()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","testContext":"CICS-REGION","TIMEOUT":30}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT", result.Config.Name);
        Assert.Equal("CICS-REGION", result.Config.TestContext);
        Assert.Equal("30", result.Config.Parameters["TIMEOUT"]);
    }

    [Fact]
    public void Parse_MalformedJson_RecordsWarning()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","timeout":""";

        var result = parser.Parse(content);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void Parse_UnrecognizedLines_AreWarned()
    {
        var parser = new ZUnitConfigParser();
        const string content = "some garbage line\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, warning => warning.Contains("garbage", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_JsonWithAlternativePropertyNames_ProducesConfig()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"configName":"ACCOUNT","context":"CICS-REGION","TIMEOUT":30}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT", result.Config.Name);
        Assert.Equal("CICS-REGION", result.Config.TestContext);
        Assert.Single(result.Config.Parameters);
    }

    [Fact]
    public void Parse_JsonMetadataProperties_AreNotParameters()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","testContext":"CTX","environment":"REG","context":"ALT"}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT", result.Config.Name);
        Assert.Equal("CTX", result.Config.TestContext);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_JsonWithEnvironmentContext_ProducesConfig()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","environment":"DEV-REGION"}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT", result.Config.Name);
        Assert.Equal("DEV-REGION", result.Config.TestContext);
    }

    [Fact]
    public void Parse_JsonBoolParameter_IsStoredAsText()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","ENABLED":true}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("True", result.Config.Parameters["ENABLED"]);
    }

    [Fact]
    public void Parse_JsonObjectParameters_AreSkipped()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","nested":{"a":1}}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_JsonNonObjectRoot_FallsBackToKeyValue()
    {
        var parser = new ZUnitConfigParser();
        const string content = """[1,2,3]""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_MalformedJsonStartingWithBrace_IsInvalid()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","timeout":""";

        var result = parser.Parse(content);

        Assert.False(result.IsValid);
        var warning = Assert.Single(result.Warnings);
        Assert.Contains("Malformed JSON", warning);
    }

    [Fact]
    public void Parse_KeyValueSectionHeader_SetsNameOnlyOnce()
    {
        var parser = new ZUnitConfigParser();
        const string content =
            "[ACCOUNT-TEST]\r\n" +
            "[IGNORED-SECTION]\r\n" +
            "KEY = value\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("ACCOUNT-TEST", result.Config.Name);
        Assert.Equal("value", result.Config.Parameters["KEY"]);
    }

    [Fact]
    public void Parse_CommentAndBlankLines_AreIgnored()
    {
        var parser = new ZUnitConfigParser();
        const string content =
            "# comment line\r\n" +
            "; also a comment\r\n" +
            "\r\n" +
            "KEY = value\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
        Assert.Equal("value", result.Config.Parameters["KEY"]);
    }

    [Fact]
    public void Parse_KeyWithoutSeparator_IsWarnedAndSkipped()
    {
        var parser = new ZUnitConfigParser();
        const string content = "just-a-key-without-value\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Contains(result.Warnings, warning => warning.Contains("just-a-key-without-value"));
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_ContextKeyOverridesInKeyValueMode()
    {
        var parser = new ZUnitConfigParser();
        const string content = "environment = QA-REGION\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("QA-REGION", result.Config.TestContext);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_ContextKeyByName_IsNotTreatedAsParameter()
    {
        var parser = new ZUnitConfigParser();
        const string content = "context = CTX-REGION\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Equal("CTX-REGION", result.Config.TestContext);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_JsonWithNonStringMetadata_DoesNotSetName()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":5}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Null(result.Config.Name);
    }

    [Fact]
    public void Parse_ValuesAreTrimmed()
    {
        var parser = new ZUnitConfigParser();
        const string content = "KEY =   padded value   \r\n";

        var result = parser.Parse(content);

        Assert.Equal("padded value", result.Config.Parameters["KEY"]);
    }

    [Fact]
    public void Parse_PropertyNameLookup_IsCaseSensitive()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"Name":"ACCOUNT","TestContext":"REGION"}""";

        var result = parser.Parse(content);

        // JSON property lookup is case-sensitive; mixed-case metadata is neither
        // read as config fields nor leaked into parameters.
        Assert.True(result.IsValid);
        Assert.Null(result.Config.Name);
        Assert.Null(result.Config.TestContext);
        Assert.Empty(result.Config.Parameters);
    }

    [Fact]
    public void Parse_NullContent_Throws()
    {
        var parser = new ZUnitConfigParser();

        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }

    [Fact]
    public void Parse_UnclosedSectionHeader_IsTreatedAsKeyValueLine()
    {
        var parser = new ZUnitConfigParser();
        const string content = "[unclosed-section\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Null(result.Config.Name);
        Assert.Contains(result.Warnings, warning => warning.Contains("unclosed-section"));
    }

    [Fact]
    public void Parse_LineStartingWithSeparator_IsWarned()
    {
        var parser = new ZUnitConfigParser();
        const string content = "=value-without-key\r\n";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Empty(result.Config.Parameters);
        Assert.Contains(result.Warnings, warning => warning.Contains("value-without-key"));
    }

    [Fact]
    public void Parse_JsonObject_DoesNotTreatMetadataAsParameters()
    {
        var parser = new ZUnitConfigParser();
        const string content = """{"name":"ACCOUNT","testContext":"CTX","KEY":"val"}""";

        var result = parser.Parse(content);

        Assert.True(result.IsValid);
        Assert.Single(result.Config.Parameters);
        Assert.Equal("val", result.Config.Parameters["KEY"]);
    }

    [Fact]
    public void Parse_EmptyContent_ProducesEmptyConfig()
    {
        var parser = new ZUnitConfigParser();

        var result = parser.Parse("");

        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Config.Parameters);
        Assert.Null(result.Config.Name);
        Assert.Null(result.Config.TestContext);
    }
}
