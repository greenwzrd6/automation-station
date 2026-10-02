
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Actions;

public interface IActionHandler
{
    string ActionType { get; }
    TargetSystem TargetSystem { get; }
    bool CanHandle(string actionType);
    Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken);
}