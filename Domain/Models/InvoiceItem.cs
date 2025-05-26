namespace Domain.Models;

public class InvoiceItem
{
    public string Id { get; set; } = null!;
    public string TicketCategory { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public decimal Amount => Price * Quantity;
}
