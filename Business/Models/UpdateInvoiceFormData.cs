using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class UpdateInvoiceFormData
{
    [Required]
    public string Id { get; set; } = null!;

    [Required]
    public string InvoiceNumber { get; set; } = null!;

    [Required]
    public DateTime IssuedDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public string BillFromName { get; set; } = null!;

    [Required]
    public string BillFromAddress { get; set; } = null!;

    [Required]
    public string BillFromEmail { get; set; } = null!;

    [Required]
    public string BillFromPhone { get; set; } = null!;

    [Required]
    public string BillToName { get; set; } = null!;

    [Required]
    public string BillToAddress { get; set; } = null!;

    [Required]
    public string BillToEmail { get; set; } = null!;

    [Required]
    public string BillToPhone { get; set; } = null!;

    [Required]
    public List<InvoiceItemUpdateFormData> Items { get; set; } = [];

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string BookingId { get; set; } = null!;

    [Required]
    public string EventId { get; set; } = null!;
}
