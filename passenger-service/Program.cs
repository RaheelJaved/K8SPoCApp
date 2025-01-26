using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Microsoft.EntityFrameworkCore;
using PassengerService.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging


        // Configure DbContext
        builder.Services.AddDbContext<PassengerContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

        // Retrieve OTLP endpoint from environment variables
        var otelEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://otel-collector:4318";



        // OpenTelemetry Configuration
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService("PassengerService"))
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint);
                    })
                    .AddConsoleExporter();
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService("PassengerService"))
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint);
                    });
            });

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        // Log the OTLP endpoint
        logger.LogInformation("OTLP Exporter Endpoint: {OtelEndpoint}", otelEndpoint);

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment()) //enable swagger for all environments
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}
