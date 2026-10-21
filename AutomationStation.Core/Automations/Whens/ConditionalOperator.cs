namespace AutomationStation.Core.Automations.Whens
{
    /// <summary>
    /// Enum <c>ConditionOperator</c> defines the comparisons used in a <see cref="Condition"/> operator.
    /// </summary>
    /// <remarks>The operator selects how the condition's <c>Value</c> is compared against the event field named by
    /// <c>Field</c>.</remarks>
    public enum ConditionOperator
    {
        Equals,

        NotEquals,
    }
}
