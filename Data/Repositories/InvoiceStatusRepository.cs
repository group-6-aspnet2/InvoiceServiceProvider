using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

namespace Data.Repositories;

public class InvoiceStatusRepository(DataContext context) : BaseRepository<InvoiceStatusEntity, InvoiceStatus>(context), IInvoiceStatusRepository
{
}
