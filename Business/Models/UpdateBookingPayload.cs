namespace Business.Models;

public class UpdateBookingPayload
{
    public string BookingId { get; set; } = null!;
    public string InvoiceId { get; set; } = null!;
}
