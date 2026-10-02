using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions;

public interface IActionExecutor
{
    Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken);
}