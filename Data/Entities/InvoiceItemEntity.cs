namespace Data.Entities;

public class InvoiceItemEntity
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public InvoiceEntity Invoice { get; set; } = null!;

    public string Category { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Price * Quantity;
}
