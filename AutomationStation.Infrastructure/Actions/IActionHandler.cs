using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Infrastructure.Actions;

public interface IActionHandler<T>
    where T : IActionContext
{
    string ActionType { get; }

    Task ExecuteAsync(
        Then then,
        T context,
        CancellationToken cancellationToken);
}