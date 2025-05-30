using Business.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Documentation;

public class UpdateInvoiceFormData_Example : IExamplesProvider<UpdateInvoiceFormData>
{
    public UpdateInvoiceFormData GetExamples() => new()
    {
        Id = "i1",
        InvoiceNumber = "INV-001",
        IssuedDate = DateTime.UtcNow,
        DueDate = DateTime.UtcNow.AddDays(30),
        BillFromName = "John Doe",
        BillFromAddress = "123 Main St, City, Country",
        BillFromEmail = "john@domain.com",
        BillFromPhone = "+1234567890",
        BillToName = "Jane Smith",
        BillToAddress = "456 Elm St, City, Country",
        BillToEmail = "jane@domain.com",
        BillToPhone = "+0987654321",
        Items =
        [
            new InvoiceItemUpdateFormData
            {
                Id = "item1",
                TicketCategory = "Gold",
                Quantity = 1,
                Price = 100.00m,
            },
        ],
        UserId = "u1",
        BookingId = "b1",
        EventId = "e1"
    };
}
