using AutomationStation.Worker;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Events;
using AutomationStation.Application.Models;
using AutomationStation.Infrastructure.Actions;
using AutomationStation.Infrastructure.Actions.Executors;
using AutomationStation.Infrastructure.Database;
using AutomationStation.Infrastructure.Integrations.Kanban;
using AutomationStation.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddSingleton<DbConnectionFactory>();

// Repository
builder.Services.AddScoped<
    IAutomationRepository,
    AutomationRepository>();

builder.Services.AddScoped<
    IHistoryRepository,
    HistoryRepository>();

// Action executor
builder.Services.AddScoped<
    IActionExecutor,
    ActionExecutor>();

builder.Services.AddScoped<
    IActionHandler<PlacementActionContext>,
    CreatePlacementExecutor>();

builder.Services.AddScoped<
    IActionHandler<ColumnActionContext>,
    CreateColumnEdgeExecutor>();

builder.Services.AddHttpClient<KanbanClient>(
    client =>
    {
        var baseUrl =
            builder.Configuration["KanbanApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Kanban API URL is missing.");

        client.BaseAddress = new Uri(baseUrl);
    });

// Event handler
builder.Services.AddTransient<
    ProcessPlacementCreatedEventHandler>();

builder.Services.AddTransient<
    ProcessColumnHasNoEdgeEventHandler>();

// RabbitMQ worker
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();