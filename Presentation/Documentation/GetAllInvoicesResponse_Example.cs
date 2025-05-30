using Swashbuckle.AspNetCore.Filters;
using Domain.Models;

namespace Presentation.Documentation;

public class GetAllInvoicesResponse_Example : IExamplesProvider<IEnumerable<Domain.Models.Invoice>>
{
    public IEnumerable<Domain.Models.Invoice> GetExamples()
    {
        return new List<Domain.Models.Invoice>
        {
            new Domain.Models.Invoice
            {
                Id = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-1001",
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
                InvoiceStatusId = 1,
                InvoiceStatus = "Unpaid",
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        TicketCategory = "Gold",
                        Price = 100,
                        Quantity = 2
                    }
                },
                UserId = "u1",
                BookingId = "b1",
                EventId = "e1"
            }
        };
    }
}
