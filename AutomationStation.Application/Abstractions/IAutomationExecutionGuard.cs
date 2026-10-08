using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions
{
    public interface IAutomationExecutionGuard
    {
        Task<bool> IsBlockedAsync(
            LoopBlockerContext context,
            CancellationToken cancellationToken);
    }
}
