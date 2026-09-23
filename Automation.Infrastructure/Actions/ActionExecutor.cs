using Automation.Application.Abstractions;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IEnumerable<IActionHandler> handlers)
    : IActionExecutor
{
    public async Task ExecuteAsync(
        Then then,
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var handler = handlers.SingleOrDefault(
            x => x.ActionType == then.Type);

        if (handler is null)
        {
            throw new NotSupportedException(
                $"Unsupported action type: {then.Type}");
        }

        await handler.ExecuteAsync(
            then,
            context,
            cancellationToken);
    }
}