namespace AutomationStation.Core.Automations.Whens
{
    /// <summary>
    /// Record <c>Condition</c> represents a condition used inside of a <see cref="When"/>.
    /// </summary>
    /// <param name="Field">The event field to check, e.g. "columnId".</param>
    /// <param name="Operator">The type of comparison to perform, see <see cref="ConditionOperator"/>.</param>
    /// <param name="Value">The expected value to compare against.</param>
    public sealed record Condition(
        string Field,
        ConditionOperator Operator,
        string Value);
}
