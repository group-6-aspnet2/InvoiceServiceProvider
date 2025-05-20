namespace Business.Models;

public class CreateInvoicePayload
{
    public string BookingId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string EventId { get; set; } = null!;
    public int TicketQuantity { get; set; }
    public decimal TicketPrice { get; set; }
    public string TicketCategoryName { get; set; } = null!;
}
