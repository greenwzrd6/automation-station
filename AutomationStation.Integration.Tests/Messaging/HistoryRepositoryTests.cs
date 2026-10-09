//using AutomationStation.Infrastructure.Persistence;
//using AutomationStation.Integration.Tests.Fixtures;
//using Dapper;

//namespace AutomationStation.Integration.Tests.Messaging
//{
//    [Collection(DatabaseCollection.Name)]
//    public sealed class HistoryRepositoryTests(DatabaseFixture database)
//    {
//        private readonly DatabaseFixture _database = database;
//        private readonly HistoryRepository _repository = new(
//            database.ConnectionFactory);

//        [Fact]
//        public async Task Creates_AutomationTimestamp_WhenUsingCreateAutomationTimestampAsync()
//        {
//             Arrange
//            var automationId = Guid.NewGuid();
//            var correlationId = Guid.NewGuid();
//            var causationId = Guid.Empty;

//            using var connection = _database.ConnectionFactory.CreateConnection();
//            await connection.OpenAsync();

//             Act
//            try
//            {
//                 Fake automation
//                await connection.ExecuteAsync(
//                    """
//                    INSERT INTO Automations
//                        (Id, Name, Enabled, AutomationTriggerable )
//                    VALUES
//                        (@AutomationId, 'Test Automation', 1, 1);
//                    """,
//                    new { AutomationId = automationId }
//                    );

//                 Insert into history
//                await _repository.CreateAutomationTimestampAsync(
//                    automationId,
//                    correlationId,
//                    causationId,
//                    CancellationToken.None);

//                 Assert
//                var returnedAutomationId = await connection.QuerySingleAsync<Guid>(
//                    """
//                    SELECT AutomationId 
//                    FROM AutomationHistory
//                    WHERE AutomationId = @AutomationId;
//                    """,
//                    new { AutomationId = automationId });

//                Assert.Equal(automationId, returnedAutomationId);

//                var returnedCorrelationId = await connection.QuerySingleAsync<Guid>(
//                    """
//                    SELECT CorrelationId 
//                    FROM AutomationHistory
//                    WHERE CorrelationId = @CorrelationId;
//                    """,
//                    new { CorrelationId = correlationId });

//                Assert.Equal(correlationId, returnedCorrelationId);
//            }
//            finally
//            {
//                Cleanup
//                await connection.ExecuteAsync(
//                    """
//                    DELETE FROM AutomationHistory
//                    WHERE AutomationId = @AutomationId;

//                    DELETE FROM Automations
//                    WHERE Id = @AutomationId;
//                    """,
//                    new { AutomationId = automationId });
//            }
//        }
//    }
//}
