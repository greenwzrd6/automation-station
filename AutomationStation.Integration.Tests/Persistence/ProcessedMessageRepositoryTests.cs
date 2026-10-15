using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;

namespace AutomationStation.Integration.Tests.Persistence
{
    [Collection(DatabaseCollection.Name)]
    public sealed class ProcessedMessageRepositoryTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;
        private readonly ProcessedMessageRepository _repository = new(
            database.ConnectionFactory);

        public async ValueTask InitializeAsync()
        {
            await _database.ResetAsync();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task MarkProcessedAsync_WhenUsing_InsertsCorrectly()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var messageId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var beforeInsert = await TimestampAsync(connection, cancellationToken);

            // Act
            await _repository.MarkProcessedAsync(messageId, cancellationToken);

            // Assert
            var afterInsert = await TimestampAsync(connection, cancellationToken);

            var processedMessage = await GetProcessedMessageRowAsync(connection, messageId, cancellationToken);

            Assert.InRange(processedMessage.ProcessedAt, beforeInsert.AddSeconds(-1), afterInsert.AddSeconds(1));

            Assert.Equal(messageId, processedMessage.Id);
        }

        [Fact]
        public async Task HasProcessedAsync_WhenUsedWithCorrectId_ReturnsTrue()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var messageId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await _repository.MarkProcessedAsync(messageId, cancellationToken);

            // Act
            var returnedValue = await _repository.HasProcessedAsync(messageId, cancellationToken);

            // Assert
            Assert.True(returnedValue);
        }

        [Fact]
        public async Task HasProcessedAsync_WhenUsedWithIncorrectId_ReturnsFalse()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var messageId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await _repository.MarkProcessedAsync(messageId, cancellationToken);

            // Act
            var returnedValue = await _repository.HasProcessedAsync(Guid.NewGuid(), cancellationToken);

            // Assert
            Assert.False(returnedValue);
        }

        private sealed record ProcessedMessageRow(
            Guid Id,
            DateTime ProcessedAt);

        private static async Task<ProcessedMessageRow> GetProcessedMessageRowAsync(
            DbConnection connection,
            Guid messageId,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<ProcessedMessageRow>(
                new CommandDefinition(
                    """
                    SELECT Id, ProcessedAt
                    FROM ProcessedMessages
                    WHERE Id = @Id
                    """,
                    new { Id = messageId },
                    cancellationToken: cancellationToken));
        }

        private static async Task<DateTime> TimestampAsync(
            DbConnection connection,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<DateTime>(
                new CommandDefinition(
                    "SELECT SYSUTCDATETIME();",
                    cancellationToken: cancellationToken));
        }
    }
}
