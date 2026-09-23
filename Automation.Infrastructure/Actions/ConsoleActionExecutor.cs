using Automation.Application.Abstractions;
using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Actions;

public sealed class ConsoleActionExecutor : IActionExecutor
{
    public Task ExecuteAsync(
        Then then,
        ActionContext context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("AUTOMATION TRIGGERED!");

        Console.WriteLine($"Action: {then.Type}");

        Console.WriteLine(
            $"Entity ID: {context.EntityId}");

        Console.WriteLine(
            $"Event ID: {context.CausationEventId}");

        foreach (var parameter in then.Parameters)
        {
            Console.WriteLine(
                $"{parameter.Key}: {parameter.Value}");
        }

        return Task.CompletedTask;
    }
}