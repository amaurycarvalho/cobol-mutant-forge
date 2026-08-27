using System.Reflection;
using System.Text.RegularExpressions;

namespace CobolMutantForge.Tests.BDD.Steps;

/// <summary>
/// Dispatches Gherkin steps to <see cref="StepAttribute"/>-annotated methods on
/// registered step-definition objects, passing captured regex groups as string
/// arguments.
/// </summary>
internal sealed class StepRunner
{
    private readonly List<RegisteredStep> _steps = new();

    public void AddSteps(object stepsObject)
    {
        ArgumentNullException.ThrowIfNull(stepsObject);

        foreach (var method in stepsObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            var attribute = method.GetCustomAttribute<StepAttribute>();
            if (attribute is null)
            {
                continue;
            }

            _steps.Add(new RegisteredStep(attribute, method, stepsObject));
        }
    }

    public void ExecuteStep(string keyword, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        ArgumentNullException.ThrowIfNull(text);

        var match = _steps.FirstOrDefault(step =>
            KeywordMatches(step.Attribute, keyword) && step.Regex.IsMatch(text));
        if (match is null)
        {
            throw new InvalidOperationException($"No step definition matched '{keyword} {text}'.");
        }

        var arguments = match.Regex.Match(text)
            .Groups
            .Cast<Group>()
            .Skip(1)
            .Select(group => group.Value)
            .ToArray();
        _ = match.Method.Invoke(match.Target, arguments);
    }

    private static bool KeywordMatches(StepAttribute attribute, string keyword)
        => keyword switch
        {
            "Given" => attribute is GivenAttribute,
            "When" => attribute is WhenAttribute,
            "Then" or "And" or "But" => attribute is ThenAttribute,
            _ => false
        };

    private sealed class RegisteredStep
    {
        public RegisteredStep(StepAttribute attribute, MethodInfo method, object target)
        {
            Attribute = attribute;
            Method = method;
            Target = target;
            Regex = new Regex(attribute.Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public StepAttribute Attribute { get; }

        public Regex Regex { get; }

        public MethodInfo Method { get; }

        public object Target { get; }
    }
}
