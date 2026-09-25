using System.Text.Json;
using System.Text.Json.Serialization;
using Automation.Application.Abstractions;
using Automation.Core.Automations;
using Automation.Infrastructure.Database;
using Automation.Infrastructure.Persistence.Models;
using Dapper;

namespace Automation.Infrastructure.Persistence;

public sealed class AutomationRepository(
    DbConnectionFactory connectionFactory)
    : IAutomationRepository
{

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

    public async Task<IReadOnlyCollection<AutomationRule>>
        GetEnabledByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken)
    {
        const string sql = """
            -- Automations and triggers
            SELECT
                a.Id AS AutomationId,
                a.Name,
                a.Enabled,
                t.EventType,
                t.SourceSystem
            FROM Automations a
            INNER JOIN AutomationTriggers t
                ON t.AutomationId = a.Id
            WHERE
                a.Enabled = 1
                AND t.EventType = @EventType;

            -- Conditions
            SELECT
                c.AutomationId,
                c.ConditionType,
                c.SourceSystem,
                c.ConfigurationJson
            FROM AutomationConditions c
            INNER JOIN Automations a
                ON a.Id = c.AutomationId
            WHERE
                a.Enabled = 1
                AND EXISTS
                (
                    SELECT 1
                    FROM AutomationTriggers t
                    WHERE
                        t.AutomationId = a.Id
                        AND t.EventType = @EventType
                );

            -- Actions
            SELECT
                act.AutomationId,
                act.ActionType,
                act.TargetSystem,
                act.ConfigurationJson,
                act.ExecutionOrder
            FROM AutomationActions act
            INNER JOIN Automations a
                ON a.Id = act.AutomationId
            WHERE
                a.Enabled = 1
                AND EXISTS
                (
                    SELECT 1
                    FROM AutomationTriggers t
                    WHERE
                        t.AutomationId = a.Id
                        AND t.EventType = @EventType
                )
            ORDER BY
                act.AutomationId,
                act.ExecutionOrder;
            """;

        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new { EventType = eventType },
                cancellationToken: cancellationToken));

        var triggers =
            (await result.ReadAsync<TriggerRow>()).ToList();

        var conditions =
            (await result.ReadAsync<ConditionRow>()).ToList();

        var actions =
            (await result.ReadAsync<ActionRow>()).ToList();

        var conditionsByAutomation = conditions
            .GroupBy(condition => condition.AutomationId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(DeserializeCondition)
                    .ToList());

        var actionsByAutomation = actions
            .GroupBy(action => action.AutomationId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(action => action.ExecutionOrder)
                    .Select(action => new Then(
                        action.ActionType,
                        DeserializeParameters(action.ConfigurationJson)))
                    .ToList());

        var automations = triggers.Select(trigger =>
        {
            var automationConditions =
                conditionsByAutomation.GetValueOrDefault(
                    trigger.AutomationId) ?? [];

            var automationActions =
                actionsByAutomation.GetValueOrDefault(
                    trigger.AutomationId) ?? [];

            return new AutomationRule(
                trigger.AutomationId,
                trigger.Name,
                trigger.Enabled,
                new When(
                    trigger.EventType,
                    trigger.SourceSystem ?? "",
                    automationConditions),
                automationActions);
        });

        return automations.ToList();
    }

    private static Condition DeserializeCondition(ConditionRow row)
    {
        if (row.ConditionType != "ColumnEquals")
        {
            throw new NotSupportedException(
                $"Unsupported condition type: {row.ConditionType}");
        }

        if (string.IsNullOrWhiteSpace(row.ConfigurationJson))
        {
            throw new InvalidOperationException(
                "Condition configuration is missing.");
        }

        return JsonSerializer.Deserialize<Condition>(
            row.ConfigurationJson,
            JsonOptions)
            ?? throw new InvalidOperationException(
                "Invalid condition configuration.");
    }

    private static Dictionary<string, string>
    DeserializeParameters(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<Dictionary<string, string>>(
            json,
            JsonOptions) ?? [];
    }
}