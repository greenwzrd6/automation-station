//using Automation.Application.Abstractions;
//using Automation.Core.Automations;
//using Automation.Infrastructure.Database;
//using Automation.Infrastructure.Persistence.Models;
//using Dapper;
//using System.Data.Common;

//namespace Automation.Infrastructure.Persistence;

//public sealed class AutomationRepository(
//    DbConnectionFactory connectionFactory) : IAutomationRepository
//{
//    public async Task<IReadOnlyCollection<AutomationRule>>
//        GetEnabledByEventTypeAsync(
//            string eventType,
//            CancellationToken cancellationToken)
//    {
//        await using var connection =
//            connectionFactory.CreateConnection();

//        await connection.OpenAsync(cancellationToken);

//        const string sql = """
//            SELECT
//                Id,
//                Name,
//                IsEnabled,
//                EventType,
//                EventSource
//            FROM Automations
//            WHERE IsEnabled = 1
//              AND EventType = @EventType
//            """;

//        var rows = await connection.QueryAsync<AutomationRow>(
//            new CommandDefinition(
//                sql,
//                new { EventType = eventType },
//                cancellationToken: cancellationToken));

//        var automations = new List<AutomationRule>();

//        foreach (var row in rows)
//        {
//            var conditions = await GetConditionsAsync(
//                connection,
//                row.Id,
//                cancellationToken);

//            var actions = await GetActionsAsync(
//                connection,
//                row.Id,
//                cancellationToken);

//            var when = new When(
//                row.EventType,
//                row.EventSource,
//                conditions);

//            automations.Add(
//                new AutomationRule(
//                    row.Id,
//                    row.Name,
//                    row.IsEnabled,
//                    when,
//                    actions));
//        }

//        return automations;
//    }

//    private static async Task<IReadOnlyCollection<Condition>>
//        GetConditionsAsync(
//            DbConnection connection,
//            Guid automationId,
//            CancellationToken cancellationToken)
//    {
//        const string sql = """
//            SELECT
//                Field,
//                Operator,
//                Value
//            FROM Conditions
//            WHERE AutomationId = @AutomationId
//            """;

//        var rows = await connection.QueryAsync<ConditionRow>(
//            new CommandDefinition(
//                sql,
//                new { AutomationId = automationId },
//                cancellationToken: cancellationToken));

//        return rows
//            .Select(row => new Condition(
//                row.Field,
//                row.Operator,
//                row.Value))
//            .ToList();
//    }

//    private static async Task<IReadOnlyCollection<Then>>
//        GetActionsAsync(
//            DbConnection connection,
//            Guid automationId,
//            CancellationToken cancellationToken)
//    {
//        const string sql = """
//            SELECT
//                Id,
//                Type
//            FROM Actions
//            WHERE AutomationId = @AutomationId
//            ORDER BY Position
//            """;

//        var rows = await connection.QueryAsync<ActionRow>(
//            new CommandDefinition(
//                sql,
//                new { AutomationId = automationId },
//                cancellationToken: cancellationToken));

//        var actions = new List<Then>();

//        foreach (var row in rows)
//        {
//            var parameters = await GetActionParametersAsync(
//                connection,
//                row.Id,
//                cancellationToken);

//            actions.Add(
//                new Then(
//                    row.Type,
//                    parameters));
//        }

//        return actions;
//    }

//    private static async Task<IReadOnlyDictionary<string, string>>
//        GetActionParametersAsync(
//            DbConnection connection,
//            Guid actionId,
//            CancellationToken cancellationToken)
//    {
//        const string sql = """
//            SELECT
//                Name,
//                Value
//            FROM ActionParameters
//            WHERE ActionId = @ActionId
//            """;

//        var rows = await connection.QueryAsync<ActionParameterRow>(
//            new CommandDefinition(
//                sql,
//                new { ActionId = actionId },
//                cancellationToken: cancellationToken));

//        return rows.ToDictionary(
//            row => row.Name,
//            row => row.Value);
//    }
//}