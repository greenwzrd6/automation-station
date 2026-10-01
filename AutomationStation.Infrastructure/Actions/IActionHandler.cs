using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Infrastructure.Actions;

public interface IActionHandler
{
    string ActionType { get; }

    Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken);
}