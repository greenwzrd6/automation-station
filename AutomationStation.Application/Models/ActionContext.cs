using AutomationStation.Application.Contracts;

namespace AutomationStation.Application.Models
{
    public sealed record ActionContext(
        IntegrationEvent Event,
        Guid ExecutionId);
}
