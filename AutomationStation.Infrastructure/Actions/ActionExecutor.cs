using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IEnumerable<IActionHandler> handlers,
    IActionRateLimiter rateLimiter)
    : IActionExecutor
{
    private readonly IReadOnlyDictionary<string, IActionHandler> _handlers =
        handlers.ToDictionary(
            handler => $"{handler.TargetSystem}:{handler.ActionType}",
            StringComparer.OrdinalIgnoreCase);

    private readonly IActionRateLimiter _rateLimiter =
        rateLimiter;

    public async Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken)
    {
        var handlerKey = $"{then.TargetSystem}:{then.Type}";
        if (!_handlers.TryGetValue(
                handlerKey,
                out var handler))
        {
            throw new NotSupportedException(
                $"Unsupported action type '{then.Type}' " +
                $"for target system '{then.TargetSystem}'.");
        }

        await _rateLimiter.WaitAsync(
            new ActionRateLimitKey(
                CompanyId: context.Event.CompanyId,
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