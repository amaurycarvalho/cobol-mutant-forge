namespace CobolMutantForge.Tests.BDD.Steps;

[AttributeUsage(AttributeTargets.Method)]
internal abstract class StepAttribute : Attribute
{
    protected StepAttribute(string pattern)
    {
        Pattern = pattern;
    }

    public string Pattern { get; }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class GivenAttribute : StepAttribute
{
    public GivenAttribute(string pattern)
        : base(pattern)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class WhenAttribute : StepAttribute
{
    public WhenAttribute(string pattern)
        : base(pattern)
    {
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class ThenAttribute : StepAttribute
{
    public ThenAttribute(string pattern)
        : base(pattern)
    {
    }
}
