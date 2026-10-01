using Microsoft.Extensions.DependencyInjection;

using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Core.Automations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IServiceProvider serviceProvider,
    IActionRateLimiter rateLimiter)
    : IActionExecutor
{
    public async Task ExecuteAsync<T>(
        Then then,
        T context,
        CancellationToken cancellationToken)
        where T : IActionContext
    {
        var _rateLimiter = rateLimiter;
        var handlers = serviceProvider.GetServices<IActionHandler<T>>();

        var handler = handlers.SingleOrDefault(
            handler =>
                handler.TargetSystem == then.TargetSystem
                &&
                handler.CanHandle(then.Type));


        if (handler is null)
        {
            throw new NotSupportedException(
                $"No handler supports action " +
                $"'{then.Type}' for target system " +
                $"'{then.TargetSystem}'.");
        }

        await _rateLimiter.WaitAsync(
            new ActionRateLimitKey(
                CompanyId: context.CompanyId,
                ActionType: then.Type,
                TargetSystem: then.TargetSystem),
            permittedActions: 100,
            window: TimeSpan.FromMinutes(1),
            cancellationToken);

        await handler.ExecuteAsync(
            then,
            context,
            cancellationToken);
    }
}