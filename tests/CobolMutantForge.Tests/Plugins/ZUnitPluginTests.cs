using CobolMutantForge.Infrastructure.Plugins;
using Xunit;

namespace CobolMutantForge.Tests.Plugins;

public class ZUnitPluginTests
{
    private const string SampleSource =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. PAYMENT.\r\n" +
        "       DATA DIVISION.\r\n" +
        "       WORKING-STORAGE SECTION.\r\n" +
        "       COPY CUSTOMER.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > 0 AND B > 0\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM PAYMENT.\r\n";

    private const string SampleXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
        "<ZUnitTestData>\r\n" +
        "  <TestCase TestCaseId=\"TC-001\">\r\n" +
        "    <Inputs>\r\n" +
        "      <CUSTOMER-ID>1001</CUSTOMER-ID>\r\n" +
        "    </Inputs>\r\n" +
        "    <ExpectedOutputs>\r\n" +
        "      <RESP-CODE>0</RESP-CODE>\r\n" +
        "    </ExpectedOutputs>\r\n" +
        "  </TestCase>\r\n" +
        "</ZUnitTestData>\r\n";

    private const string SampleConfig =
        "[PAYMENT-TEST]\r\n" +
        "testContext = CICS-REGION\r\n" +
        "TIMEOUT = 30\r\n";

    [Fact]
    public void Import_SampleExportDirectory_ProducesValidResult()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);
            File.WriteAllText(Path.Combine(directory, "tests.xml"), SampleXml);
            File.WriteAllText(Path.Combine(directory, "config.bzucfg"), SampleConfig);
            Directory.CreateDirectory(Path.Combine(directory, "copybooks"));
            File.WriteAllText(Path.Combine(directory, "copybooks", "CUSTOMER.cpy"), "       01 CUSTOMER.\r\n");

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            Assert.Empty(result.Warnings);
            var program = Assert.Single(result.Programs);
            Assert.Equal("PAYMENT", program.Name);
            Assert.Contains("CUSTOMER", program.Copybooks);
            var testCase = Assert.Single(result.TestCases);
            Assert.Equal("TC-001", testCase.Id);
            Assert.Equal("CICS-REGION", result.Config.TestContext);
            Assert.Contains("CUSTOMER.cpy", result.Copybooks);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_MissingCopybook_RecordsWarningWithoutHardFailing()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            Assert.Contains(result.Warnings, warning => warning.Contains("CUSTOMER", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_MissingDirectory_IsInvalid()
    {
        var plugin = new ZUnitPlugin();
        var missing = Path.Combine(Path.GetTempPath(), "cmf-missing-" + Guid.NewGuid().ToString("N"));

        var result = plugin.Import(missing);

        Assert.False(result.IsValid);
        var warning = Assert.Single(result.Warnings);
        Assert.Contains("directory not found", warning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Import_BlankInputPath_Throws()
    {
        var plugin = new ZUnitPlugin();

        Assert.Throws<ArgumentException>(() => plugin.Import(" "));
    }

    [Fact]
    public void Import_EmptyDirectory_IsValidWithNoArtifacts()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            Assert.Empty(result.Warnings);
            Assert.Empty(result.Programs);
            Assert.Empty(result.TestCases);
            Assert.Empty(result.Copybooks);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_ProgramNameComesFromProgramIdNotFileName()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "WEIRDFILENAME.cbl"),
                "       IDENTIFICATION DIVISION.\r\n" +
                "       PROGRAM-ID. PAYMENT.\r\n" +
                "       PROCEDURE DIVISION.\r\n" +
                "           DISPLAY 'HI'\r\n" +
                "       END-PROGRAM PAYMENT.\r\n");

            var result = plugin.Import(directory);

            var program = Assert.Single(result.Programs);
            Assert.Equal("PAYMENT", program.Name);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_ProgramWithoutProgramId_UsesFileName()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "MYPROG.cbl"),
                "       IDENTIFICATION DIVISION.\r\n" +
                "       PROCEDURE DIVISION.\r\n" +
                "           DISPLAY 'HI'\r\n" +
                "       END-PROGRAM.\r\n");

            var result = plugin.Import(directory);

            var program = Assert.Single(result.Programs);
            Assert.Equal("MYPROG", program.Name);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_CopybookResolvedFromConfiguredDirectory()
    {
        var directory = CreateTempDirectory();
        try
        {
            var configuredCopybooks = Path.Combine(directory, "configured-copybooks");
            Directory.CreateDirectory(configuredCopybooks);
            File.WriteAllText(Path.Combine(configuredCopybooks, "CUSTOMER.cpy"), "       01 CUSTOMER.\r\n");
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);
            var plugin = new ZUnitPlugin(configuredCopybooks);

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            var program = Assert.Single(result.Programs);
            Assert.Contains("CUSTOMER", program.Copybooks);
            Assert.Contains("CUSTOMER.cpy", result.Copybooks);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_CopybookLocatedInSourceDirectory_IsDiscovered()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "CUSTOMER.cpy"), "       01 CUSTOMER.\r\n");
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            Assert.Contains("CUSTOMER.cpy", result.Copybooks);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_MissingBzucfg_UsesDefaultConfig()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);

            var result = plugin.Import(directory);

            Assert.True(result.IsValid);
            Assert.Null(result.Config.Name);
            Assert.Null(result.Config.TestContext);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_MalformedBzucfg_IsInvalidButDoesNotThrow()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);
            File.WriteAllText(Path.Combine(directory, "config.bzucfg"), """{"broken":""");

            var result = plugin.Import(directory);

            Assert.False(result.IsValid);
            Assert.Contains(result.Warnings, warning => warning.Contains("JSON", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Import_ExposesPluginNameAndVersion()
    {
        var plugin = new ZUnitPlugin();

        Assert.Equal("zunit", plugin.Name);
        Assert.Equal("1.0.0", plugin.Version);
    }

    [Fact]
    public void Import_MalformedXml_IsInvalidButDoesNotThrow()
    {
        var plugin = new ZUnitPlugin();
        var directory = CreateTempDirectory();
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "PAYMENT.cbl"), SampleSource);
            File.WriteAllText(Path.Combine(directory, "broken.xml"), "<ZUnitTestData><TestCase>");

            var result = plugin.Import(directory);

            Assert.False(result.IsValid);
            Assert.Contains(result.Warnings, warning => warning.Contains("XML", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
        => Path.Combine(Path.GetTempPath(), "cmf-test-" + Guid.NewGuid().ToString("N"));
}
