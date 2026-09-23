using Automation.Application.Abstractions;
using Automation.Application.Events;
using Automation.Infrastructure.Actions;
using Automation.Infrastructure.Persistence;
using Automation.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
//builder.Services.AddInfrastructure(
//    builder.Configuration);

builder.Services.AddSingleton<
    IAutomationRepository,
    InMemoryAutomationRepository>();

builder.Services.AddHttpClient<
    IActionExecutor,
    ActionExecutor>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["KanbanApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Kanban API URL is missing."));
    });

builder.Services.AddTransient<
    ProcessPlacementCreatedEventHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();