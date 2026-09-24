using Automation.Core.Automations;

namespace Automation.Application.Abstractions;

public interface IActionExecutor
{
    Task ExecuteAsync<T>(
        Then then,
        T context,
        CancellationToken cancellationToken)
        where T : IActionContext;
}