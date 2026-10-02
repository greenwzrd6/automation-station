using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IEnumerable<IActionHandler> handlers)
    : IActionExecutor
{
    private readonly IReadOnlyDictionary<string, IActionHandler> _handlers =
        handlers.ToDictionary(
            handler => handler.ActionType,
            StringComparer.OrdinalIgnoreCase);

    public async Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(
                then.Type,
                out var handler))
        {
            throw new NotSupportedException(
                $"Unsupported action type '{then.Type}'.");
        }

        await handler.ExecuteAsync(
            then,
            context,
            cancellationToken);
    }
}