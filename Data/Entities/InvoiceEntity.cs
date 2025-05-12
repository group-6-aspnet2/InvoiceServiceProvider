using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class InvoiceEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
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

    [ForeignKey(nameof(InvoiceStatus))]
    public int InvoiceStatusId { get; set; }
    public virtual InvoiceStatusEntity InvoiceStatus { get; set; } = null!;

    public virtual ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = [];

    public string UserId { get; set; } = null!;
    public string BookingId { get; set; } = null!;
    public string EventId { get; set; } = null!;

    public decimal Total => InvoiceItems?.Sum(i => i.Amount) ?? 0m;
}
