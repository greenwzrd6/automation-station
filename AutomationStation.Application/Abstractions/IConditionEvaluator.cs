using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations.Whens;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Evaluates whether an integration event satisfies a single automation condition.
    /// </summary>
    /// <remarks>Used by the automation processor to check each <see cref="Condition"/> in a
    /// <see cref="When"/> against the event that arrived. All conditions of a <c>When</c> must match for the
    /// automation's trigger to fire.</remarks>
    public interface IConditionEvaluator
    {
        /// <summary>
        /// Checks whether the given event satisfies the given condition.
        /// </summary>
        /// <param name="condition">The condition to evaluate, consisting of a field name, a
        /// <see cref="ConditionOperator"/>, and the expected value.</param>
        /// <param name="integrationEvent">The event whose payload is compared against the condition.</param>
        /// <returns><see langword="true"/> if the event satisfies the condition; otherwise <see langword="false"/>.</returns>
        bool Matches(
            Condition condition,
            IntegrationEvent integrationEvent);
    }
}
