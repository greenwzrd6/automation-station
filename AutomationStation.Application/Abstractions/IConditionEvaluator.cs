using AutomationStation.Application.Contracts;
using AutomationStation.Core.Automations.Whens;

namespace AutomationStation.Application.Abstractions
{
    public interface IConditionEvaluator
    {
        bool Matches(
            Condition condition,
            IntegrationEvent integrationEvent);
    }
}
