using Business.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Documentation;

public class CreateInvoicePayload_Example : IExamplesProvider<CreateInvoicePayload>
{
    public CreateInvoicePayload GetExamples() => new()
    {
        BookingId = "b1",
        UserId = "u1",
        EventId = "e1",
        TicketQuantity = 2,
        TicketPrice = 50.00m,
        TicketCategoryName = "Platinum"
    };
}
