using Azure.Messaging.ServiceBus;
using Business.Interfaces;
using Business.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Business.Services;

public class InvoiceQueueBackgroundService : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<UpdateBookingWithInvoiceIdHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public InvoiceQueueBackgroundService(ServiceBusClient client, IConfiguration configuration, ILogger<UpdateBookingWithInvoiceIdHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _processor = client.CreateProcessor(configuration["AzureServiceBusSettings:QueueName"], new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += ErrorHandler;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var payload = JsonSerializer.Deserialize<CreateInvoicePayload>(body);

            using var scope = _scopeFactory.CreateScope();
            var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

            var result = await invoiceService.CreateInvoiceAsync(payload!);

            if (result.Succeeded)
            {
                await args.CompleteMessageAsync(args.Message);
            }
            else
            {
                _logger.LogWarning("Create invoice failed: {error}", result.Error);
                await args.AbandonMessageAsync(args.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message");
            await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message handler encountered an exception");
        return Task.CompletedTask;
    }
}
