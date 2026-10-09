using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Policies;
using AutomationStation.Application.Services;
using AutomationStation.Infrastructure.Actions;
using AutomationStation.Infrastructure.Actions.Executors;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Integrations.Kanban;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Infrastructure.RateLimiting;
using AutomationStation.Infrastructure.Integrations.Toj;
using AutomationStation.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
builder.Services.AddScoped<
    IAutomationRepository,
    AutomationRepository>();

builder.Services.AddScoped<HistoryRepository>();

builder.Services.AddScoped<IHistoryRepository>(
    services => services.GetRequiredService<HistoryRepository>());

builder.Services.AddScoped<
    IProcessedMessageRepository,
    ProcessedMessageRepository>();

builder.Services.AddScoped<
    IAutomationExecutionLimiter,
    AutomationExecutionLimiter>();

// Action catalog
builder.Services.AddSingleton<
    IActionCatalog,
    ActionCatalog>();

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

builder.Services.AddScoped<
    IActionHandler,
    CreateTojTaskExecutor>();

builder.Services.AddScoped<
    IActionHandler,
    UpdateTojTaskStatusExecutor>();

builder.Services.AddHttpClient<KanbanClient>(
    client =>
    {
        var baseUrl =
            builder.Configuration["KanbanApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Kanban API URL is missing.");

        client.BaseAddress = new Uri(baseUrl);
    });
builder.Services.AddHttpClient<TojClient>(
    (serviceProvider, client) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var baseUrl =
            configuration["TojApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Toj:BaseUrl is missing.");

        var apiKey =
            configuration["TojApi:ApiKey"]
            ?? throw new InvalidOperationException(
                "Toj:ApiKey is missing.");


        client.BaseAddress = new Uri(baseUrl);

        client.DefaultRequestHeaders.Add(
            "X-Automation-Api-Key",
            apiKey);
    });

// RabbitMQ worker
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();