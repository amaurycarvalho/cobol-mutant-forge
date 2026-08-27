using CobolMutantForge.Application.Services;
using CobolMutantForge.Domain.Entities;
using CobolMutantForge.Domain.ValueObjects;
using Xunit;

namespace CobolMutantForge.Tests.Application;

public class ValidationServiceTests
{
    private const string Source =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. PAYMENT.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM PAYMENT.\r\n";

    [Fact]
    public void IsApplicable_NullProgram_Throws()
    {
        var service = new ValidationService();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.Throws<ArgumentNullException>(() => service.IsApplicable(null!, mutation));
    }

    [Fact]
    public void IsApplicable_NullMutation_Throws()
    {
        var service = new ValidationService();
        var program = CreateProgram();

        Assert.Throws<ArgumentNullException>(() => service.IsApplicable(program, null!));
    }

    [Fact]
    public void IsApplicable_NoOpMutation_ReturnsFalse()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "AND");

        Assert.False(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_LineAtLowerBoundary_ReturnsTrueWhenTextPresent()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 1, "IDENTIFICATION", "MUTATED");

        // The 'mutation.Line < 1' guard must not reject line 1.
        Assert.True(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_LineAtUpperBoundary_ReturnsTrueWhenTextPresent()
    {
        var service = new ValidationService();
        var program = CreateProgramWithoutTrailingNewline();
        var mutation = new Mutation("m1", MutationType.AndToOr, 7, "END-PROGRAM", "MUTATED");

        // The 'mutation.Line > lines.Length' guard must not reject the last line.
        Assert.True(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_LineBeyondSourceLength_ReturnsFalse()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 100, "AND", "OR");

        Assert.False(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_TextPresentOnLine_ReturnsTrue()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "AND", "OR");

        Assert.True(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_TextMissingFromLine_ReturnsFalse()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 4, "OR", "AND");

        Assert.False(service.IsApplicable(program, mutation));
    }

    [Fact]
    public void IsApplicable_TextPresentOnAnySourceLine_ReturnsTrue()
    {
        var service = new ValidationService();
        var program = CreateProgram();
        var mutation = new Mutation("m1", MutationType.AndToOr, 5, "+", "-");

        Assert.True(service.IsApplicable(program, mutation));
    }

    private const string BoundarySource =
        "       IDENTIFICATION DIVISION.\r\n" +
        "       PROGRAM-ID. PAYMENT.\r\n" +
        "       PROCEDURE DIVISION.\r\n" +
        "           IF A > B AND C = D\r\n" +
        "               COMPUTE TOTAL = AMOUNT + TAX\r\n" +
        "           END-IF.\r\n" +
        "       END-PROGRAM PAYMENT.";

    private static CobolProgram CreateProgram()
        => new("PAYMENT", Source);

    private static CobolProgram CreateProgramWithoutTrailingNewline()
        => new("PAYMENT", BoundarySource);
}
