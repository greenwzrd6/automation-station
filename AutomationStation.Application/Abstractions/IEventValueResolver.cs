using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Abstractions
{
    public interface IEventValueResolver
    {
        bool TryGetValue(
            IntegrationEvent integrationEvent,
            string field,
            out string? value);
    }
}
