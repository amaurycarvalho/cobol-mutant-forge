namespace CobolMutantForge.Domain.Interfaces;

public sealed record ParseDiagnostic(DiagnosticSeverity Severity, string Message, int Line, int Column);
