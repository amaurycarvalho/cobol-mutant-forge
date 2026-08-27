using CobolMutantForge.Infrastructure.Serialization;
using Xunit;

namespace CobolMutantForge.Tests.Serialization;

public class ZUnitXmlParserTests
{
    private const string SampleXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
        "<ZUnitTestData>\r\n" +
        "  <TestCase TestCaseId=\"TC-001\">\r\n" +
        "    <Inputs>\r\n" +
        "      <CUSTOMER-ID>1001</CUSTOMER-ID>\r\n" +
        "      <AMOUNT>250.00</AMOUNT>\r\n" +
        "    </Inputs>\r\n" +
        "    <ExpectedOutputs>\r\n" +
        "      <BALANCE>750.00</BALANCE>\r\n" +
        "      <RESP-CODE>0</RESP-CODE>\r\n" +
        "    </ExpectedOutputs>\r\n" +
        "  </TestCase>\r\n" +
        "</ZUnitTestData>\r\n";

    [Fact]
    public void Parse_ValidXml_MaterializesTestCaseWithInputsAndExpectedOutputs()
    {
        var parser = new ZUnitXmlParser();

        var result = parser.Parse(SampleXml);

        var testCase = Assert.Single(result.TestCases);
        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
        Assert.Equal("TC-001", testCase.Id);
        Assert.Equal("1001", testCase.Inputs["CUSTOMER-ID"]);
        Assert.Equal("250.00", testCase.Inputs["AMOUNT"]);
        Assert.Equal("0", testCase.ExpectedOutputs["RESP-CODE"]);
    }

    [Fact]
    public void Parse_MalformedXml_RecordsWarningAndDoesNotThrow()
    {
        var parser = new ZUnitXmlParser();

        var result = parser.Parse("<ZUnitTestData><TestCase>");

        Assert.False(result.IsValid);
        Assert.Empty(result.TestCases);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void Parse_XmlWithoutTestCases_RecordsWarning()
    {
        var parser = new ZUnitXmlParser();

        var result = parser.Parse("<ZUnitTestData><Foo>bar</Foo></ZUnitTestData>");

        Assert.True(result.IsValid);
        Assert.Empty(result.TestCases);
        Assert.Contains(result.Warnings, warning => warning.Contains("test case", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_TestCaseWithoutId_GeneratesId()
    {
        var parser = new ZUnitXmlParser();
        const string xml = "<TestCase><Inputs><A>1</A></Inputs></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.False(string.IsNullOrWhiteSpace(testCase.Id));
        Assert.StartsWith("TestCase-", testCase.Id);
    }

    [Theory]
    [InlineData("TestRecord")]
    [InlineData("Test")]
    [InlineData("Record")]
    public void Parse_AlternativeTestCaseElementNames_AreRecognized(string elementName)
    {
        var parser = new ZUnitXmlParser();
        var xml = $"<Root><{elementName} id=\"TC-1\"><Inputs><A>1</A></Inputs></{elementName}></Root>";

        var result = parser.Parse(xml);

        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("TC-1", testCase.Id);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("name")]
    [InlineData("testname")]
    [InlineData("testcaseid")]
    public void Parse_TestCaseIdAttribute_IsCaseInsensitive(string attributeName)
    {
        var parser = new ZUnitXmlParser();
        var xml = $"<TestCase {attributeName}=\"TC-1\"><Inputs><A>1</A></Inputs></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("TC-1", testCase.Id);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("TestName")]
    [InlineData("TestCaseId")]
    public void Parse_TestCaseIdChildElement_IsRecognized(string childName)
    {
        var parser = new ZUnitXmlParser();
        var xml = $"<TestCase><{childName}>TC-9</{childName}><Inputs><A>1</A></Inputs></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("TC-9", testCase.Id);
    }

    [Fact]
    public void Parse_AlternativeInputContainerNames_AreRecognized()
    {
        var parser = new ZUnitXmlParser();
        const string xml = "<TestCase id=\"TC-1\"><TestData><A>1</A></TestData><Input><B>2</B></Input></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("1", testCase.Inputs["A"]);
        Assert.Equal("2", testCase.Inputs["B"]);
    }

    [Fact]
    public void Parse_AlternativeExpectedOutputContainerNames_AreRecognized()
    {
        var parser = new ZUnitXmlParser();
        const string xml =
            "<TestCase id=\"TC-1\">" +
            "<Expected><X>1</X></Expected>" +
            "<Outputs><Y>2</Y></Outputs>" +
            "<Output><Z>3</Z></Output>" +
            "</TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("1", testCase.ExpectedOutputs["X"]);
        Assert.Equal("2", testCase.ExpectedOutputs["Y"]);
        Assert.Equal("3", testCase.ExpectedOutputs["Z"]);
    }

    [Fact]
    public void Parse_MultipleTestCases_AreAllMaterialized()
    {
        var parser = new ZUnitXmlParser();
        const string xml =
            "<Root>" +
            "<TestCase id=\"TC-1\"><Inputs><A>1</A></Inputs></TestCase>" +
            "<TestCase id=\"TC-2\"><Inputs><A>2</A></Inputs></TestCase>" +
            "</Root>";

        var result = parser.Parse(xml);

        Assert.Equal(2, result.TestCases.Count);
        Assert.Contains(result.TestCases, testCase => testCase.Id == "TC-1");
        Assert.Contains(result.TestCases, testCase => testCase.Id == "TC-2");
    }

    [Fact]
    public void Parse_InputValues_AreTrimmed()
    {
        var parser = new ZUnitXmlParser();
        const string xml = "<TestCase id=\"TC-1\"><Inputs><A>   padded  </A></Inputs></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("padded", testCase.Inputs["A"]);
    }

    [Fact]
    public void Parse_NonContainerChildElements_AreIgnored()
    {
        var parser = new ZUnitXmlParser();
        const string xml = "<TestCase id=\"TC-1\"><Inputs><A>1</A></Inputs><OTHER>ignored</OTHER></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Empty(testCase.ExpectedOutputs);
    }

    [Fact]
    public void Parse_NestedElementInsideContainer_IsIgnored()
    {
        var parser = new ZUnitXmlParser();
        const string xml = "<TestCase id=\"TC-1\"><Inputs><A><B>1</B></A></Inputs></TestCase>";

        var result = parser.Parse(xml);

        var testCase = Assert.Single(result.TestCases);
        Assert.Equal("1", testCase.Inputs["A"]);
    }

    [Fact]
    public void Parse_NullXml_Throws()
    {
        var parser = new ZUnitXmlParser();

        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }
}
