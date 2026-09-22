using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Application.Abstractions;

public interface IActionExecutor
{
    Task ExecuteAsync(
        Then then,
        ActionContext context,
        CancellationToken cancellationToken);
}