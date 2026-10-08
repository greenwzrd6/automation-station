using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Abstractions
{
    public interface IActionHandler
    {
        string ActionType { get; }
        TargetSystem TargetSystem { get; }

        Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken);
    }
}