using Automation.Application.Abstractions;
using Automation.Core.Automations;
using Automation.Infrastructure.Database;

namespace Automation.Infrastructure.Persistence
{
    public class AutomationRepository(DbConnectionFactory connectionFactory) : IAutomationRepository
    {
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;

        public Task<IReadOnlyCollection<AutomationRule>> GetEnabledByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            throw new NotImplementedException();
        }
    }
}