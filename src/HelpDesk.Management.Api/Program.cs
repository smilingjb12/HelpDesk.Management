using Asp.Versioning;
using FluentValidation;
using HelpDesk.Management.Application.Authentication;
using HelpDesk.Management.Application.Incidents.Commands;
using HelpDesk.Management.Application.Incidents.Projections;
using HelpDesk.Management.Domain.Incidents;
using HelpDesk.Management.Domain.Incidents.Validation;
using HelpDesk.Management.Infrastructure.Authentication;
using HelpDesk.Management.Infrastructure.Incidents;
using Marten;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HelpDesk Management API",
        Version = "v1"
    });
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LogIncidentCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(LogIncidentCommand).Assembly);
builder.Services.AddControllers();

builder.Services.AddMarten((StoreOptions opts) =>
{
    opts.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    opts.Projections.Add<IncidentHistoryProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);

    // Register the Incident projection as an event-sourced aggregat
    //opts.Projections.Add<Incident>(Marten.Events.Projections.ProjectionLifecycle.Async);
});

// Add API versioning services
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add these services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddScoped<IIncidentValidator, IncidentValidator>();
builder.Services.AddScoped<IIncidentRepository, DocumentStoreIncidentRepository>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HelpDesk Management API V1");
    });
}

// Update the middleware order
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers after all middleware
app.MapControllers();

app.Use(async (context, next) =>
{
    Debug.WriteLine($"Request Path: {context.Request.Path}");
    await next();
});

// Apply any pending database changes on startup
using (var scope = app.Services.CreateScope())
{
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
}

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run();
