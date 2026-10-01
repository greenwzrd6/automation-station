using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;

namespace AutomationStation.Infrastructure.Actions;

public interface IActionHandler<T>
    where T : IActionContext
{
    string ActionType { get; }
    TargetSystem TargetSystem { get; }
    bool CanHandle(string actionType);
    Task ExecuteAsync(
        Then then,
        T context,
        CancellationToken cancellationToken);
}