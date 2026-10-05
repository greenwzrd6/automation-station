using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Policies;
using AutomationStation.Application.Services;
using AutomationStation.Infrastructure.Actions;
using AutomationStation.Infrastructure.Actions.Executors;
using AutomationStation.Infrastructure.Blockers;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Integrations.Kanban;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Worker;
using AutomationStation.Infrastructure.RateLimiting;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
builder.Services.AddScoped<
    IAutomationRepository,
    AutomationRepository>();

builder.Services.AddScoped<
    IHistoryRepository,
    HistoryRepository>();

builder.Services.AddScoped<
    IProcessedMessageRepository,
    ProcessedMessageRepository>();

builder.Services.AddScoped<
    ICorrelationLoopRepository,
    CorrelationLoopRepository>();

// Action catalog
builder.Services.AddSingleton<
    IActionCatalog,
    ActionCatalog>();

// Event blockers
builder.Services.AddScoped<
    IEventBlocker,
    CorrelationLoopBlocker>();

// Generic automation engine
builder.Services.AddScoped<
    IAutomationProcessor,
    AutomationProcessor>();

builder.Services.AddScoped<
    IAutomationExecutionGuard,
    AutomationExecutionGuard>();

builder.Services.AddScoped<
    IAutomationExecutionRepository,
    AutomationExecutionRepository>();

builder.Services.AddScoped<
    IConditionEvaluator,
    ConditionEvaluator>();

builder.Services.AddScoped<
    IEventValueResolver,
    EventValueResolver>();

// Generic execution policies

builder.Services.AddScoped<
    IAutomationExecutionPolicy,
    CorrelationPolicy>();

builder.Services.AddScoped<
    IAutomationExecutionPolicy,
    AutomationTriggerablePolicy>();

// Rate limiting
builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);

builder.Services.AddSingleton<
    IActionRateLimiter,
    InMemoryActionRateLimiter>();

// Actions
builder.Services.AddScoped<
    IActionExecutor,
    ActionExecutor>();

builder.Services.AddScoped<
    IActionHandler,
    CreatePlacementExecutor>();

builder.Services.AddHttpClient<KanbanClient>(
    client =>
    {
        var baseUrl =
            builder.Configuration["KanbanApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Kanban API URL is missing.");

        client.BaseAddress = new Uri(baseUrl);
    });

// RabbitMQ worker
builder.Services.AddHostedService<Worker>();

// Cleanup old correlation loop events
builder.Services.AddHostedService<CorrelationLoopCleanupService>();

await builder.Build().RunAsync();