using Automation.Infrastructure.Database;

public class AutomationRepository(DbConnectionFactory connectionFactory)
{
    private readonly DbConnectionFactory _connectionFactory = connectionFactory;
}