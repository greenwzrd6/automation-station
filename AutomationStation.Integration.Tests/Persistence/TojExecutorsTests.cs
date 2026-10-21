using Dapper;

using Microsoft.Data.SqlClient;

using AutomationStation.Core.Automations.Systems;
using AutomationStation.Integration.Tests.Fixtures;

namespace AutomationStation.Integration.Tests.Persistence
{
    [Collection(DatabaseCollection.Name)]
    public class TojExecutorsTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;

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
        public async Task InsertAction_WithMissingExecutorId_IsRejectedByForeignKey()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var missingExecutorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(
                """
                INSERT INTO Automations (Id, Name)
                VALUES (@Id, @Name);
                """,
                new { Id = automationId, Name = "Test automation" },
                transaction);

            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO AutomationActions
                            (Id, AutomationId, AutomationExecutorId, ActionType)
                        VALUES
                            (@Id, @AutomationId, @ExecutorId, @ActionType);
                        """,
                        new
                        {
                            Id = actionId,
                            AutomationId = automationId,
                            ExecutorId = missingExecutorId,
                            ActionType = "CreateTask"
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken)));
            Assert.Equal(547, exception.Number);
        }

        [Fact]
        public async Task InsertAction_WithValidExecutorId_IsInsertedSuccessfully()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations (Id, Name)
                    VALUES (@Id, @Name);
                    """,
                    new { Id = automationId, Name = "Test automation" },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES (@Id, 1, @TargetSystem, @Name, 1);    
                    """,
                    new
                    {
                        Id = executorId,
                        TargetSystem = TargetSystem.TojSystem,
                        Name = "Test executor"
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var count = await connection.ExecuteAsync(
                new CommandDefinition(
                """
                INSERT INTO AutomationActions
                    (Id, AutomationId, AutomationExecutorId, ActionType)
                VALUES
                    (@Id, @AutomationId, @ExecutorId, @ActionType);
                """,
                new
                {
                    Id = actionId,
                    AutomationId = automationId,
                    ExecutorId = executorId,
                    ActionType = "CreateTask"
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task InsertExecutor_WithDuplicateId_IsRejectedByPrimaryKey()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@Id, 1, @TargetSystem, @Name, 1);
                    """,
                    new
                    {
                        Id = executorId,
                        TargetSystem = TargetSystem.TojSystem,
                        Name = "Test executor"
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO AutomationExecutors
                            (Id, CompanyId, TargetSystem, Name, Enabled)
                        VALUES
                            (@Id, 1, @TargetSystem, @Name, 1);
                        """,
                        new
                        {
                            Id = executorId,
                            TargetSystem = TargetSystem.TojSystem,
                            Name = "Duplicate executor"
                        },
                    transaction: transaction,
                    cancellationToken: cancellationToken)));

            Assert.Equal(2627, exception.Number);
        }

        [Fact]
        public async Task InsertExecutor_WithValidData_IsInsertedSuccessfully()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            
            var count = await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO AutomationExecutors
                        (Id, CompanyId, TargetSystem, Name, Enabled)
                    VALUES
                        (@Id, 1, @TargetSystem, @Name, 1);
                    """,
                    new
                    {
                        Id = executorId,
                        TargetSystem = TargetSystem.TojSystem,
                        Name = "Test executor"
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task InsertExecutor_WithNullCompanyId_IsRejectedByNotNullConstraint()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO AutomationExecutors
                            (Id, CompanyId, TargetSystem, Name, Enabled)
                        VALUES
                            (@Id, null, @TargetSystem, @Name, 1);
                        """,
                        new { Id = executorId, TargetSystem = TargetSystem.TojSystem, Name = "Test executor" },
                        transaction: transaction,
                        cancellationToken: cancellationToken)));

            Assert.Equal(515, exception.Number);
        }

        [Fact]
        public async Task InsertExecutor_WithNullTargetSystem_IsRejectedByNotNullConstraint()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();
            
            using var connection = _database.ConnectionFactory.CreateConnection();   
            await connection.OpenAsync(cancellationToken);
            
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            
            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO AutomationExecutors
                            (Id, CompanyId, TargetSystem, Name, Enabled)
                        VALUES
                            (@Id, 1, null, @Name, 1);
                        """,
                        new { Id = executorId, Name = "Test executor" },
                        transaction: transaction,
                        cancellationToken: cancellationToken)));

            Assert.Equal(515, exception.Number);
        }
    }
}
