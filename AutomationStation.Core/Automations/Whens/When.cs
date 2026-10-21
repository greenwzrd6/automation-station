using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations.Whens
{
    /// <summary>
    /// Record <c>When</c> represents what causes an automation to happen.
    /// </summary>
    /// <param name="Trigger">What triggers the <c>When</c>, see <see cref="Trigger"/>.</param>
    /// <param name="Conditions">The condition(s) that must be met for an automation to execute, see <see cref="Condition"/>.</param>
    public sealed record When(
        Trigger Trigger,
        IReadOnlyCollection<Condition> Conditions);
}
