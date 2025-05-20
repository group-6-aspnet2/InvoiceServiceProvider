using Azure.Messaging.ServiceBus;
using Business.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Business.Services;

public class CreateInvoiceQueueBackgroundService(ServiceBusProcessor processor, IInvoiceServiceBusHandler handler) : IHostedService, ICreateInvoiceQueueBackgroundService
{
    private readonly ServiceBusProcessor _processor = processor;
    private readonly IInvoiceServiceBusHandler _handler = handler;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _processor.ProcessMessageAsync += _handler.HandleMessageAsync;
        _processor.ProcessErrorAsync += _handler.HandleErrorAsync;
        return _processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _processor.StopProcessingAsync(cancellationToken);
    }
}
