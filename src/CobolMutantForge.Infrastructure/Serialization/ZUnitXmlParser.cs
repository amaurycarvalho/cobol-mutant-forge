using System.Xml;
using System.Xml.Linq;
using CobolMutantForge.Domain.Entities;

namespace CobolMutantForge.Infrastructure.Serialization;

public sealed record ZUnitXmlParseResult
{
    public IReadOnlyList<TestCase> TestCases { get; init; } = Array.Empty<TestCase>();

    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    public bool IsValid { get; init; } = true;
}

/// <summary>
/// Tolerant parser for ZUnit <c>.xml</c> test data exports.
///
/// ZUnit's XML format is undocumented (PRD risk), so this parser is deliberately
/// permissive: it treats elements named <c>TestCase</c>/<c>TestRecord</c>/<c>Test</c>/
/// <c>Record</c> as test case records, reads the id from a recognized attribute or
/// child element, and maps the child elements of an <c>Inputs</c> and
/// <c>ExpectedOutputs</c> container onto the <see cref="TestCase"/> inputs and
/// expected outputs. Malformed input records a warning instead of throwing, so a
/// single bad export never aborts an import.
/// </summary>
public sealed class ZUnitXmlParser
{
    public ZUnitXmlParseResult Parse(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch (XmlException exception)
        {
            return new ZUnitXmlParseResult
            {
                IsValid = false,
                Warnings = new[] { $"Malformed ZUnit XML: {exception.Message}" }
            };
        }

        var testCases = document.Descendants()
            .Where(IsTestCaseElement)
            .Select(CreateTestCase)
            .ToList();

        if (testCases.Count == 0)
        {
            return new ZUnitXmlParseResult
            {
                Warnings = new[] { "No ZUnit test case elements were found in the XML document." }
            };
        }

        return new ZUnitXmlParseResult { TestCases = testCases };
    }

    private static bool IsTestCaseElement(XElement element)
    {
        var name = element.Name.LocalName;
        return name.Equals("TestCase", StringComparison.OrdinalIgnoreCase)
            || name.Equals("TestRecord", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Test", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Record", StringComparison.OrdinalIgnoreCase);
    }

    private static TestCase CreateTestCase(XElement element)
    {
        var id = ReadId(element);
        var inputs = ReadDataMap(element, "Inputs", "Input", "TestData");
        var expectedOutputs = ReadDataMap(element, "ExpectedOutputs", "Expected", "Outputs", "Output");
        return new TestCase(id, inputs, expectedOutputs);
    }

    private static string ReadId(XElement element)
    {
        var attributeId = FirstAttributeValue(element, "id", "name", "testname", "testcaseid");
        if (attributeId is not null)
        {
            return attributeId;
        }

        var childId = FirstChildElement(element, "Name", "TestName", "TestCaseId");
        if (childId is not null)
        {
            return childId.Value.Trim();
        }

        return $"TestCase-{Guid.NewGuid():N}";
    }

    private static string? FirstAttributeValue(XElement element, params string[] names)
    {
        foreach (var attribute in element.Attributes())
        {
            if (names.Any(name => attribute.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return attribute.Value.Trim();
            }
        }

        return null;
    }

    private static XElement? FirstChildElement(XElement element, params string[] names)
    {
        foreach (var child in element.Elements())
        {
            if (names.Any(name => child.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return child;
            }
        }

        return null;
    }

    private static Dictionary<string, string> ReadDataMap(XElement element, params string[] containerNames)
    {
        var result = new Dictionary<string, string>();
        foreach (var container in element.Elements())
        {
            if (!containerNames.Any(name => container.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            foreach (var child in container.Elements())
            {
                result[child.Name.LocalName] = child.Value.Trim();
            }
        }

        return result;
    }
}
