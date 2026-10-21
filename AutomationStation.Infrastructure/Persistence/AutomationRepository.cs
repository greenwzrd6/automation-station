using Dapper;

using System.Text.Json;
using System.Text.Json.Serialization;

using AutomationStation.Application.Abstractions;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Core.Automations.Whens;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Models.Rows;

namespace AutomationStation.Infrastructure.Persistence
{
    /// <summary>
    /// Loads full automation aggregates from the database, including their triggers, conditions, and actions.
    /// </summary>
    /// <remarks>Automations are stored relationally (Automations, AutomationTriggers, AutomationConditions,
    /// AutomationActions) while condition configuration and action parameters are persisted as JSON. Condition JSON is
    /// deserialized with web naming conventions and string enums; action parameters are read as a string dictionary.
    /// Actions are returned ordered by their execution order.</remarks>
    /// <param name="connectionFactory">Factory that provides connections to the automation database.</param>
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

        /// <summary>
        /// Loads every enabled automation that has a trigger for the given event type, with its full trigger,
        /// condition, and action configuration.
        /// </summary>
        /// <remarks>A single round-trip returns four result sets (automations, triggers, conditions, actions) that are
        /// stitched together in memory into <see cref="Automation"/> aggregates. Only automations with
        /// <c>Enabled = 1</c> and at least one trigger on <paramref name="eventType"/> are returned. Condition rows
        /// whose configuration JSON is missing or invalid cause an <see cref="InvalidOperationException"/> rather than
        /// silently dropping the condition.</remarks>
        /// <param name="eventType">The event type to match against automation triggers, compared exactly as stored
        /// (case-sensitive).</param>
        /// <param name="cancellationToken">A token used to cancel the database calls.</param>
        /// <returns>A task that completes with the matching automation aggregates, each with exactly one trigger, its
        /// conditions, and its actions ordered by execution order.</returns>
        /// <exception cref="InvalidOperationException">An enabled automation has no trigger (or more than one), an
        /// action has no target system, or a condition's configuration JSON is missing or cannot be deserialized.</exception>
        public async Task<IReadOnlyCollection<Automation>> GetEnabledByEventTypeAsync(
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

            var automationRows = (await result.ReadAsync<AutomationRow>()).ToList();

            var triggerRows = (await result.ReadAsync<TriggerRow>()).ToList();

            var conditionRows = (await result.ReadAsync<ConditionRow>()).ToList();

            var actionRows = (await result.ReadAsync<ActionRow>()).ToList();

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
                    conditionsByAutomation.GetValueOrDefault(automation.Id)
                    ?? [];

                var automationActions =
                    actionsByAutomation.GetValueOrDefault(automation.Id)
                    ?? [];

                return new Automation(
                    automation.Id,
                    automation.Name,
                    automation.Enabled,
                    automation.AutomationTriggerable,
                    new When(
                        new Trigger(
                            automationTrigger.EventType,
                            automationTrigger.SourceSystem),
                        automationConditions),
                    automationActions);
            });

            return [.. automations];
        }

        // Deserializes a condition row's configuration JSON; throws when the JSON is missing or invalid.
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