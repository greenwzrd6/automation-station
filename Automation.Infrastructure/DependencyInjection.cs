//using Automation.Application.Abstractions;
//using Automation.Infrastructure.Actions;
//using Automation.Infrastructure.Actions.Executors;
//using Automation.Infrastructure.Database;
//using Automation.Infrastructure.Integrations.Planning;
//using Automation.Infrastructure.Persistence;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace Automation.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructure(
//        this IServiceCollection services,
//        IConfiguration configuration)
//    {
//        var connectionString =
//            configuration.GetConnectionString(
//                "DefaultConnection")
//            ?? throw new InvalidOperationException(
//                "Connection string not found.");

//        services.AddSingleton(
//            new DbConnectionFactory(connectionString));

//        services.AddScoped<
//            IAutomationRepository,
//            AutomationRepository>();

//        services.AddScoped<
//            IActionExecutor,
//            ActionExecutor>();

//        services.AddScoped<
//            IActionHandler,
//            CreatePlacementExecutor>();

//        services.AddHttpClient<PlanningClient>(
//            client =>
//            {
//                var baseUrl =
//                    configuration["Planning:BaseUrl"]
//                    ?? throw new InvalidOperationException(
//                        "Planning BaseUrl is missing.");

//                client.BaseAddress = new Uri(baseUrl);
//            });

//        return services;
//    }
//}