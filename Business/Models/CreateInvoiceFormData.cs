namespace Business.Models;

public class CreateInvoiceFormData
{
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

    public List<InvoiceItemFormData> Items { get; set; } = [];
}
