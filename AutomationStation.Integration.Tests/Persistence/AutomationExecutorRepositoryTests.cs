using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;

namespace AutomationStation.Integration.Tests.Persistence
{
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationExecutorRepositoryTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;
        private readonly AutomationExecutorRepository _repository = new(
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
        public async Task GetByIdAsync_WhenExecutorExists_ShouldReturnExecutor()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationExecutorAsync(connection, executorId, true, cancellationToken);

            // Act
            var executor = await _repository.GetByIdAsync(executorId, cancellationToken);

            // Assert
            Assert.NotNull(executor);

            Assert.Equal(executorId, executor.Id);

            Assert.Equal(1, executor.CompanyId);

            Assert.True(executor.Enabled);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExecutorDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Act
            var executor = await _repository.GetByIdAsync(Guid.NewGuid(), cancellationToken);

            // Assert
            Assert.Null(executor);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExecutorIsDisabled_ShouldReturnNull()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationExecutorAsync(connection, executorId, false, cancellationToken);

            // Act
            var executor = await _repository.GetByIdAsync(executorId, cancellationToken);

            // Assert
            Assert.Null(executor);
        }

        private static async Task InsertAutomationExecutorAsync(
            DbConnection connection,
            Guid executorId,
            bool enabled,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@ExecutorId, 1, 'KanbanSystem', 'ExecutorTest', @Enabled)
                    """,
                    new
                    {
                        ExecutorId = executorId,
                        Enabled = enabled
                    },
                    cancellationToken: cancellationToken));
        }
    }
}
