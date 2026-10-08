using Dapper;
using System.Text.Json;
using System.Text.Json.Serialization;

using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Persistence.Models;

namespace AutomationStation.Infrastructure.Persistence
{
    public sealed class AutomationRepository(
        DbConnectionFactory connectionFactory)
        : IAutomationRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web)
            {
                Converters =
                {
                new JsonStringEnumConverter()
                }
            };

        public async Task<IReadOnlyCollection<Automation>>
            GetEnabledByEventTypeAsync(
                string eventType,
                CancellationToken cancellationToken)
        {
            const string sql = """
            -- Automations
            SELECT
                a.Id,
                a.Name,
                a.Enabled,
                a.AutomationTriggerable
            FROM Automations a
            WHERE a.Enabled = 1
              AND EXISTS (
                  SELECT 1
                  FROM AutomationTriggers t
                  WHERE t.AutomationId = a.Id
                    AND t.EventType = @EventType
              );

            -- Trigger
            SELECT
                t.AutomationId,
                t.EventType,
                t.SourceSystem
            FROM AutomationTriggers t
            INNER JOIN Automations a
                ON a.Id = t.AutomationId
            WHERE a.Enabled = 1
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
                act.ExecutionOrder,
                act.AutomationExecutorId AS ExecutorId
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

            await using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            using var result = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    sql,
                    new { EventType = eventType },
                    cancellationToken: cancellationToken));

            var automationRows =
                (await result.ReadAsync<AutomationRow>()).ToList();

            var triggerRows =
                (await result.ReadAsync<TriggerRow>()).ToList();

            var conditionRows =
                (await result.ReadAsync<ConditionRow>()).ToList();

            var actionRows =
                (await result.ReadAsync<ActionRow>()).ToList();

            var triggersByAutomation = triggerRows
                .GroupBy(trigger => trigger.AutomationId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());

            var conditionsByAutomation = conditionRows
                .GroupBy(condition => condition.AutomationId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(DeserializeCondition)
                        .ToList());

            var actionsByAutomation = actionRows
                .GroupBy(action => action.AutomationId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(action => action.ExecutionOrder)
                        .Select(action => new Then(
                            action.ActionType,
                            action.TargetSystem ?? throw new InvalidOperationException(
                            $"Action '{action.ActionType}' " +
                            $"in automation '{action.AutomationId}' " +
                            $"has no TargetSystem."),
                            action.ExecutorId,
                            DeserializeParameters(action.ConfigurationJson)))
                        .ToList());

            var automations = automationRows.Select(automation =>
            {
                var automationTrigger =
                    triggersByAutomation.GetValueOrDefault(automation.Id)
                        ?.SingleOrDefault()
                    ?? throw new InvalidOperationException(
                        $"Automation {automation.Id} has no trigger.");

                var automationConditions =
                    conditionsByAutomation.GetValueOrDefault(
                        automation.Id) ?? [];

                var automationActions =
                    actionsByAutomation.GetValueOrDefault(
                        automation.Id) ?? [];

                return new Automation(
                    automation.Id,
                    automation.Name,
                    automation.Enabled,
                    automation.AutomationTriggerable,
                    new When(
                        automationTrigger.EventType,
                        automationTrigger.SourceSystem,
                        automationConditions),
                    automationActions);
            });

            return [.. automations];
        }

        private static Condition DeserializeCondition(
            ConditionRow row)
        {
            if (string.IsNullOrWhiteSpace(
                    row.ConfigurationJson))
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
}