using System.ComponentModel.DataAnnotations;

namespace Business.Models;

public class InvoiceItemUpdateFormData
{
    [Required]
    public string Id { get; set; } = null!;

    [Required]
    public string TicketCategory { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Quantity { get; set; }
}