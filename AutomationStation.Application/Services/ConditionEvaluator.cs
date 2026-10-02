using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Services
{
    public sealed class ConditionEvaluator(
        IEventValueResolver valueResolver)
        : IConditionEvaluator
    {
        private readonly IEventValueResolver _valueResolver = valueResolver;

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
