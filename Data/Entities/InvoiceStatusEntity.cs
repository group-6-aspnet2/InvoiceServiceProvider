namespace Data.Entities;

public class InvoiceStatusEntity
{
    public int Id { get; set; }
    public string StatusName { get; set; } = null!;

    public virtual ICollection<InvoiceEntity> Invoices { get; set; } = [];
}
