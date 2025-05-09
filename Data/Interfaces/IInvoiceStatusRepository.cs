using Data.Entities;
using Domain.Models;

namespace Data.Interfaces;

public interface IInvoiceStatusRepository : IBaseRepository<InvoiceStatusEntity, InvoiceStatus>
{
}