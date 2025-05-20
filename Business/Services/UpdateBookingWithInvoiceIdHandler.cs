using Azure.Messaging.ServiceBus;
using Business.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Business.Services;

public class UpdateBookingWithInvoiceIdHandler : IUpdateBookingWithInvoiceIdHandler
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public UpdateBookingWithInvoiceIdHandler(IConfiguration configuration)
    {
        _client = new ServiceBusClient(configuration["AzureServiceBusSettings:ConnectionString"]);
        _sender = _client.CreateSender(configuration["AzureServiceBusSettings:ResponseQueueName"]);
    }

    public async Task PublishAsync(string payload)
    {
        var message = new ServiceBusMessage(payload);
        Console.WriteLine($"Sending message: {payload}");
        await _sender.SendMessageAsync(message);
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
