namespace Data.Entities;

public class InvoiceItemEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int InvoiceId { get; set; }
    public InvoiceEntity Invoice { get; set; } = null!;

    public string TicketCategory { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Price * Quantity;
}
