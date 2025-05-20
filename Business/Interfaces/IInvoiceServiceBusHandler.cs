using Azure.Messaging.ServiceBus;

namespace Business.Interfaces;

public interface IInvoiceServiceBusHandler
{
    Task HandleErrorAsync(ProcessErrorEventArgs args);
    Task HandleMessageAsync(ProcessMessageEventArgs args);
}