using Data.Entities;
using Domain.Models;

namespace Data.Interfaces;

public interface IInvoiceRepository : IBaseRepository<InvoiceEntity, Invoice>
{
}
