using Azure.Messaging.ServiceBus;
using Business.Interfaces;
using Business.Models;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;

namespace Business.Services;

public class InvoiceServiceBusHandler(IInvoiceService invoiceService, ILogger<InvoiceServiceBusHandler> logger) : IInvoiceServiceBusHandler
{
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly ILogger<InvoiceServiceBusHandler> _logger = logger;

    public async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var json = args.Message.Body.ToString();

            var payload = JsonSerializer.Deserialize<CreatedBookingPayload>(json)
                ?? throw new InvalidOperationException("Invalid message format.");

            var invoiceForm = new CreateInvoiceFormData
            {
                InvoiceNumber = $"INVOICE-{payload.BookingId}",
                IssuedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                BillFromName = "MyCompany",
                BillFromAddress = "Street 123, City",
                BillFromEmail = "petra@domain.com",
                BillFromPhone = "+46701234567",
                BillToName = "-",
                BillToAddress = string.Empty,
                BillToEmail = string.Empty,
                BillToPhone = string.Empty,
                UserId = payload.UserId,
                BookingId = payload.BookingId,
                EventId = payload.EventId,
                Items = new List<InvoiceItemFormData> {
                    new InvoiceItemFormData
                    {
                        TicketCategory = "Standard",
                        Price = payload.TicketPrice,
                        Quantity = payload.TicketQuantity
                    }
                }
            };

            var result = await _invoiceService.CreateInvoiceAsync(invoiceForm);

            if (!result.Succeeded)
                _logger.LogError($"Failed to create invoice for booking {payload.BookingId}: {result.Error}");

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling booking message from queue.");
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    public Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error.");
        return Task.CompletedTask;
    }
}
