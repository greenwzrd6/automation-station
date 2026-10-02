using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationProcessor
    {
        Task ProcessAsync(
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken);
    }
}
