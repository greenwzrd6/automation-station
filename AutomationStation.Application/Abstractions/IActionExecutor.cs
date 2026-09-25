using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Abstractions;

public interface IActionExecutor
{
    Task ExecuteAsync<T>(
        Then then,
        T context,
        CancellationToken cancellationToken)
        where T : IActionContext;
}