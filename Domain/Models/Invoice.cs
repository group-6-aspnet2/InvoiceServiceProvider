namespace Domain.Models;

public class Invoice
{
    public string Id { get; set; } = null!;
    public string InvoiceNumber { get; set; } = null!;
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }

    public string BillFromName { get; set; } = null!;
    public string BillFromAddress { get; set; } = null!;
    public string BillFromEmail { get; set; } = null!;
    public string BillFromPhone { get; set; } = null!;

    public string BillToName { get; set; } = null!;
    public string BillToAddress { get; set; } = null!;
    public string BillToEmail { get; set; } = null!;
    public string BillToPhone { get; set; } = null!;

    public int InvoiceStatusId { get; set; }
    public string InvoiceStatus { get; set; } = null!;

    public List<InvoiceItem> Items { get; set; } = [];

    public string UserId { get; set; } = null!;
    public string BookingId { get; set; } = null!;
    public string EventId { get; set; } = null!;

    public decimal Total => Items?.Sum(i => i.Amount) ?? 0;
}
