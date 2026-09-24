using Automation.Application.Abstractions;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Actions;

public interface IActionHandler<T>
    where T : IActionContext
{
    string ActionType { get; }

    Task ExecuteAsync(
        Then then,
        T context,
        CancellationToken cancellationToken);
}