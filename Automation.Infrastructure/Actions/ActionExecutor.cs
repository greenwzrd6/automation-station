using Automation.Application.Abstractions;
using Automation.Core.Automations;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IServiceProvider serviceProvider)
    : IActionExecutor
{
    public async Task ExecuteAsync<T>(
        Then then,
        T context,
        CancellationToken cancellationToken)
        where T : IActionContext
    {
        var handlers = serviceProvider.GetServices<IActionHandler<T>>();

        var handler = handlers.SingleOrDefault(
            h => h.ActionType == then.Type);


        if (handler is null)
        {
            throw new NotSupportedException(
                $"Unsupported action type: '{then.Type}' " +
                $"for context '{typeof(T).Name}'");
        }

        await handler.ExecuteAsync(
            then,
            context,
            cancellationToken);
    }
}