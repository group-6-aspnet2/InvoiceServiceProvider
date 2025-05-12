using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class InvoiceStatusEntity
{
    [Key]
    public int Id { get; set; }
    public string StatusName { get; set; } = null!;

    public virtual ICollection<InvoiceEntity> Invoices { get; set; } = [];
}
