using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations.Whens;

namespace AutomationStation.Application.Services
{
    /// <summary>
    /// Evaluates a single <see cref="Condition"/> against an integration event for <see cref="IConditionEvaluator"/>.
    /// </summary>
    /// <remarks>The condition's field is resolved through <see cref="IEventValueResolver"/>, so a condition whose
    /// field the event does not carry never matches, regardless of its operator. Comparisons are case-insensitive
    /// and performed on the string representation of the value; any operator outside
    /// <see cref="ConditionOperator.Equals"/> and <see cref="ConditionOperator.NotEquals"/> evaluates to false.
    /// The evaluator is stateless and safe to use concurrently.</remarks>
    /// <param name="valueResolver">Resolver used to read the condition's field from the event.</param>
    public sealed class ConditionEvaluator(
        IEventValueResolver valueResolver)
        : IConditionEvaluator
    {
        private readonly IEventValueResolver _valueResolver = valueResolver;

        /// <summary>
        /// Determines whether the given event satisfies the given condition.
        /// </summary>
        /// <param name="condition">The condition to evaluate, naming the event field, the comparison to perform
        /// and the expected value.</param>
        /// <param name="integrationEvent">The event whose resolved field value is compared against the condition.</param>
        /// <returns>true when the field resolves and the comparison succeeds; false when the field is missing, the
        /// comparison fails, or the operator is not recognised.</returns>
        public bool Matches(
            Condition condition,
            IntegrationEvent integrationEvent)
        {
            var found = _valueResolver.TryGetValue(
                integrationEvent,
                condition.Field,
                out var actualValue);

            if (!found)
            {
                return false;
            }

            var result = condition.Operator switch
            {
                ConditionOperator.Equals =>
                    string.Equals(
                        actualValue,
                        condition.Value,
                        StringComparison.OrdinalIgnoreCase),

                ConditionOperator.NotEquals =>
                    !string.Equals(
                        actualValue,
                        condition.Value,
                        StringComparison.OrdinalIgnoreCase),

                _ => false
            };

            return result;
        }
    }
}
