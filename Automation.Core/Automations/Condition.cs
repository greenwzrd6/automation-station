namespace Automation.Core.Automations
{
    public sealed record Condition(
        string Field,
        ConditionOperator Operator,
        string Value);
}
