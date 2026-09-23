using Automation.Application.Abstractions;
using Automation.Application.Events;
using Automation.Infrastructure.Actions;
using Automation.Infrastructure.Database;
using Automation.Infrastructure.Persistence;
using Automation.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repository
builder.Services.AddScoped<
    IAutomationRepository,
    AutomationRepository>();

// Action executor
builder.Services.AddHttpClient<
    IActionExecutor,
    ActionExecutor>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["KanbanApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Kanban API URL is missing."));
    });

// Event handler
builder.Services.AddTransient<
    ProcessPlacementCreatedEventHandler>();

// RabbitMQ worker
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();