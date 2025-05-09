namespace Business.Models;

public class InvoiceItemFormData
{
    public string TicketCategory { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
