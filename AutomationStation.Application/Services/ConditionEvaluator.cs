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
            if (!_valueResolver.TryGetValue(
                    integrationEvent,
                    condition.Field,
                    out var actualValue))
            {
                return false;
            }

            return condition.Operator switch
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
        }
    }
}
