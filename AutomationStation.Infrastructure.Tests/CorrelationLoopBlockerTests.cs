using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Blockers;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace AutomationStation.Infrastructure.Tests;

public class CorrelationLoopBlockerTests : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly ICorrelationLoopRepository _repository;
    private readonly IEventBlocker _blocker;
    private readonly List<Guid> _createdCorrelationIds = [];

    public CorrelationLoopBlockerTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _connectionFactory = new DbConnectionFactory(configuration);
        _repository = new CorrelationLoopRepository(_connectionFactory);
        _blocker = new CorrelationLoopBlocker(_repository);

        EnsureTableExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureTableExistsAsync()
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM sys.tables WHERE name = 'CorrelationLoopEvents'
            )
            BEGIN
                CREATE TABLE CorrelationLoopEvents (
                    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    CorrelationId UNIQUEIDENTIFIER NOT NULL,
                    EventId UNIQUEIDENTIFIER NOT NULL,
                    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );

                CREATE INDEX IX_CorrelationLoopEvents_CorrelationId_ReceivedAt
                ON CorrelationLoopEvents(CorrelationId, ReceivedAt);

                CREATE UNIQUE INDEX UX_CorrelationLoopEvents_CorrelationId_EventId
                ON CorrelationLoopEvents(CorrelationId, EventId);
            END
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql);
    }

    public void Dispose()
    {
        foreach (var correlationId in _createdCorrelationIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            connection.Execute(
                "DELETE FROM CorrelationLoopEvents WHERE CorrelationId = @CorrelationId",
                new { CorrelationId = correlationId });
        }
    }

    [Fact]
    public async Task Allows_10_Events_With_Same_CorrelationId()
    {
        var correlationId = TrackCorrelationId();

        for (int i = 0; i < 10; i++)
        {
            var context = CreateContext(correlationId);
            var isBlocked = await _blocker.IsBlockedAsync(context, CancellationToken.None);

            Assert.False(isBlocked);
        }
    }

    [Fact]
    public async Task Blocks_11th_Event_With_Same_CorrelationId()
    {
        var correlationId = TrackCorrelationId();

        for (int i = 0; i < 10; i++)
        {
            await _blocker.IsBlockedAsync(CreateContext(correlationId), CancellationToken.None);
        }

        var isBlocked = await _blocker.IsBlockedAsync(
            CreateContext(correlationId),
            CancellationToken.None);

        Assert.True(isBlocked);
    }

    [Fact]
    public async Task Different_CorrelationIds_Do_Not_Block_Each_Other()
    {
        var correlationId1 = TrackCorrelationId();
        var correlationId2 = TrackCorrelationId();

        for (int i = 0; i < 11; i++)
        {
            await _blocker.IsBlockedAsync(CreateContext(correlationId1), CancellationToken.None);
        }

        var isBlocked = await _blocker.IsBlockedAsync(
            CreateContext(correlationId2),
            CancellationToken.None);

        Assert.False(isBlocked);
    }

    private Guid TrackCorrelationId()
    {
        var correlationId = Guid.NewGuid();
        _createdCorrelationIds.Add(correlationId);
        return correlationId;
    }

    private static EventBlockerContext CreateContext(Guid correlationId)
    {
        return new EventBlockerContext(
            EventId: Guid.NewGuid(),
            EventType: "PlacementCreated",
            Source: SourceSystem.Kanban,
            CorrelationId: correlationId,
            CausationEventId: null);
    }
}
