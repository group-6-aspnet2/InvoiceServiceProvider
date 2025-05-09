namespace Business.Models;

public class InvoiceItemUpdateFormData
{
    public string Id { get; set; } = null!;
    public string TicketCategory { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}