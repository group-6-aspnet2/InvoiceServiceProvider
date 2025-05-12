using Business.Models;
using Domain.Models;
using Domain.Responses;

namespace Business.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResult> ChangeStatusAsync(string id, string status);
        Task<InvoiceResult<Invoice>> CreateInvoiceAsync(CreateInvoiceFormData formData);
        Task<InvoiceResult<IEnumerable<Invoice>>> GetAllAsync();
        Task<InvoiceResult<Invoice>> GetByIdAsync(string id);
        Task<InvoiceResult<IEnumerable<Invoice>>> GetByStatusIdAsync(int statusId);
        Task<InvoiceResult> HoldAsync(string id);
        Task<InvoiceResult> MarkAsPaidAsync(string id);
        Task<InvoiceResult> MarkAsUnpaidAsync(string id);
        Task<InvoiceResult> SendAsync(string id);
        Task<InvoiceResult<Invoice>> UpdateAsync(UpdateInvoiceFormData formData);
    }
}