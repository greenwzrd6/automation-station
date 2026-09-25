namespace AutomationStation.Core.Automations
{
    public sealed record Condition(
        string Field,
        ConditionOperator Operator,
        string Value);
}
