using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;

namespace Data.Repositories;

public class InvoiceRepository(DataContext context) : BaseRepository<InvoiceEntity, Invoice>(context), IInvoiceRepository
{
}
