using Business.Interfaces;
using Business.Models;
using Data.Contexts;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;
using Domain.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Business.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository, IInvoiceStatusRepository invoiceStatusRepository, IUpdateBookingWithInvoiceIdHandler bookingServiceBusHandler, DataContext context) : IInvoiceService
{
    private readonly DataContext _context;
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly IInvoiceStatusRepository _invoiceStatusRepository = invoiceStatusRepository;
    private readonly IUpdateBookingWithInvoiceIdHandler _bookingServiceBusHandler = bookingServiceBusHandler;
    private readonly BookingManager.BookingManagerClient _bookingClient;
    private readonly EventContract.EventContractClient _eventClient;
    private readonly AccountGrpcService.AccountGrpcServiceClient _accountClient;

    public async Task<InvoiceResult<Invoice>> CreateInvoiceAsync(CreateInvoicePayload formData)
    {
        try
        {
            var bookingResult = await _bookingClient.GetOneBookingAsync(new GetOneBookingRequest { BookingId = formData.BookingId });
            var eventResult = await _eventClient.GetEventByIdAsync(new GetEventByIdRequest { EventId = formData.EventId });
            var accountResult = await _accountClient.GetAccountByIdAsync(new GetAccountByIdRequest { UserId = formData.UserId });

            if (formData == null)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 400, Error = "Invalid invoice form." };

            var newInvoiceId = Guid.NewGuid().ToString();

            var invoiceEntity = new InvoiceEntity
            {
                Id = newInvoiceId,
                InvoiceNumber = $"INV-{formData.BookingId}",
                IssuedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                BillFromName = "EPN Sverige AB",
                BillFromAddress = "Nordkapsvägen 1, 136 57 Vega",
                BillFromEmail = "epnsverige@domain.com",
                BillFromPhone = "+46 707 123 4567",
                BillToName = accountResult.Account.UserName, // TODO: Be Olivia lägga till full name i Account
                //BillToAddress = accountResult.Account.Address, TODO: Be Olivia lägga till address i Account
                BillToEmail = accountResult.Account.Email,
                BillToPhone = accountResult.Account.PhoneNumber,
                InvoiceStatusId = 1,
                InvoiceItems = [ new InvoiceItemEntity // TODO: Se om denna casten är korrekt?
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceId = newInvoiceId,
                    TicketCategory = formData.TicketCategoryName,
                    Price = 100,
                    Quantity = 1
                }],
                UserId = formData.UserId,
                BookingId = formData.BookingId,
                EventId = formData.EventId
            };

            var result = await _invoiceRepository.AddAsync(invoiceEntity);
            if (result.Succeeded)
            {
                var payload = JsonSerializer.Serialize(new UpdateBookingPayload
                {
                    BookingId = formData.BookingId,
                    InvoiceId = "234-45564",
                });

                await _bookingServiceBusHandler.PublishAsync(payload);
            }

            return result.Succeeded
                ? new InvoiceResult<Invoice> { Succeeded = true, StatusCode = result.StatusCode, Result = result.Result }
                : new InvoiceResult<Invoice> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<InvoiceResult<IEnumerable<Invoice>>> GetAllAsync()
    {
        try
        {
            var entities = await _context.Invoices
                .Include(i => i.InvoiceStatus)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();

            var models = entities.Select(MapEntityToModel).ToList();

            return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = true, StatusCode = 200, Result = models };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }


        //try
        //{
        //    //var result = await _invoiceRepository.GetAllAsync(
        //    //    orderByDescending: true,
        //    //    sortBy: i => i.IssuedDate,
        //    //    where: null,
        //    //    take: 0,
        //    //    includes: [x => x.InvoiceItems, x => x.InvoiceStatus]
        //    //    );

        //    if (!result.Succeeded)
        //        return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

        //    return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = true, StatusCode = result.StatusCode, Result = result.Result };
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine(ex.Message);
        //    return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        //}
    }

    public async Task<InvoiceResult<Invoice>> GetByIdAsync(string id)
    {
        try
        {
            var entity = await _context.Invoices
                .Include(i => i.InvoiceStatus)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (entity == null)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 404, Error = $"Invoice with id '{id}' not found." };

            var model = MapEntityToModel(entity);

            return new InvoiceResult<Invoice> { Succeeded = true, StatusCode = 200, Result = model };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }


        //try
        //{
        //    var result = await _invoiceRepository.GetAsync(
        //        where: i => i.Id == id,
        //        includes: [x => x.InvoiceItems, x => x.InvoiceStatus]
        //        );

        //    if (!result.Succeeded)
        //        return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

        //    return new InvoiceResult<Invoice> { Succeeded = true, StatusCode = result.StatusCode, Result = result.Result };
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine(ex.Message);
        //    return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        //}
    }

    public async Task<InvoiceResult<IEnumerable<Invoice>>> GetByStatusIdAsync(int statusId)
    {
        try
        {
            var result = await _invoiceRepository.GetAllAsync(
                orderByDescending: true,
                sortBy: i => i.IssuedDate,
                where: i => i.InvoiceStatusId == statusId,
                take: 0,
                includes: [x => x.InvoiceItems, x => x.InvoiceStatus]
                );

            if (!result.Succeeded)
                return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = true, StatusCode = result.StatusCode, Result = result.Result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<IEnumerable<Invoice>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public async Task<InvoiceResult<Invoice>> UpdateAsync(UpdateInvoiceFormData formData)
    {
        try
        {
            if (formData == null)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 400, Error = "Invalid invoice form." };

            var existingInvoice = await _invoiceRepository.GetAsync(
                x => x.Id == formData.Id,
                includes: x => x.InvoiceItems
                );

            if (!existingInvoice.Succeeded || existingInvoice.Result == null)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 404, Error = $"Invoice with id '{formData.Id}' not found." };

            var updatedInvoice = existingInvoice.Result.MapTo<InvoiceEntity>();

            updatedInvoice.InvoiceNumber = formData.InvoiceNumber;
            updatedInvoice.IssuedDate = formData.IssuedDate;
            updatedInvoice.DueDate = formData.DueDate;
            updatedInvoice.BillFromName = formData.BillFromName;
            updatedInvoice.BillFromAddress = formData.BillFromAddress;
            updatedInvoice.BillFromEmail = formData.BillFromEmail;
            updatedInvoice.BillFromPhone = formData.BillFromPhone;
            updatedInvoice.BillToName = formData.BillToName;
            updatedInvoice.BillToAddress = formData.BillToAddress;
            updatedInvoice.BillToEmail = formData.BillToEmail;
            updatedInvoice.BillToPhone = formData.BillToPhone;
            updatedInvoice.UserId = formData.UserId;
            updatedInvoice.BookingId = formData.BookingId;
            updatedInvoice.EventId = formData.EventId;

            var existingItems = formData.Items;

            var invoiceItemsToRemove = updatedInvoice.InvoiceItems
                .Where(i => !existingItems.Any(i => i.Id == i.Id))
                .ToList();
            foreach (var item in invoiceItemsToRemove)
                updatedInvoice.InvoiceItems.Remove(item);

            foreach (var itemForm in existingItems)
            {
                if (itemForm.Id != string.Empty)
                {
                    var existingItem = updatedInvoice.InvoiceItems.First(i => i.Id == itemForm.Id);
                    existingItem.TicketCategory = itemForm.TicketCategory;
                    existingItem.Price = itemForm.Price;
                    existingItem.Quantity = itemForm.Quantity;
                }
                else
                {
                    updatedInvoice.InvoiceItems.Add(new InvoiceItemEntity
                    {
                        Id = Guid.NewGuid().ToString(),
                        TicketCategory = itemForm.TicketCategory,
                        Price = itemForm.Price,
                        Quantity = itemForm.Quantity
                    });
                }
            }

            var updateResult = await _invoiceRepository.UpdateAsync(updatedInvoice);
            return updateResult.Succeeded
                ? new InvoiceResult<Invoice> { Succeeded = true, StatusCode = updateResult.StatusCode, Result = updateResult.Result }
                : new InvoiceResult<Invoice> { Succeeded = false, StatusCode = updateResult.StatusCode, Error = updateResult.Error };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public Task<InvoiceResult> SendAsync(string id) => ChangeStatusAsync(id, "Sent");
    public Task<InvoiceResult> HoldAsync(string id) => ChangeStatusAsync(id, "Held");
    public Task<InvoiceResult> MarkAsPaidAsync(string id) => ChangeStatusAsync(id, "Paid");
    public Task<InvoiceResult> MarkAsUnpaidAsync(string id) => ChangeStatusAsync(id, "Unpaid");

    public async Task<InvoiceResult> ChangeStatusAsync(string id, string status)
    {
        try
        {
            var invoice = await _invoiceRepository.GetAsync(x => x.Id == id);
            if (!invoice.Succeeded || invoice.Result == null)
                return new InvoiceResult { Succeeded = false, StatusCode = invoice.StatusCode, Error = invoice.Error ?? $"Invoice with id '{id}' not found." };

            var invoiceStatuses = await _invoiceStatusRepository.GetAllAsync();
            if (!invoiceStatuses.Succeeded || invoiceStatuses.Result == null)
                return new InvoiceResult { Succeeded = false, StatusCode = invoiceStatuses.StatusCode, Error = invoiceStatuses.Error ?? "Invoice statuses not found." };

            var target = invoiceStatuses.Result.FirstOrDefault(x => x.StatusName.Equals(status, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                return new InvoiceResult { Succeeded = false, StatusCode = 404, Error = $"Invoice status '{status}' not found." };

            var invoiceEntity = invoice.MapTo<InvoiceEntity>();
            invoiceEntity.InvoiceStatusId = target.Id;

            var result = await _invoiceRepository.UpdateAsync(invoiceEntity);
            if (!result.Succeeded)
                return new InvoiceResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            return new InvoiceResult { Succeeded = true, StatusCode = result.StatusCode };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }


    private Domain.Models.Invoice MapEntityToModel(Data.Entities.InvoiceEntity e)
    {
        return new Domain.Models.Invoice
        {
            Id = e.Id,
            InvoiceNumber = e.InvoiceNumber,
            IssuedDate = e.IssuedDate,
            DueDate = e.DueDate,
            BillFromName = e.BillFromName,
            BillFromAddress = e.BillFromAddress,
            BillFromEmail = e.BillFromEmail,
            BillFromPhone = e.BillFromPhone,
            BillToName = e.BillToName,
            BillToAddress = e.BillToAddress,
            BillToEmail = e.BillToEmail,
            BillToPhone = e.BillToPhone,
            InvoiceStatusId = e.InvoiceStatusId,
            InvoiceStatus = e.InvoiceStatus?.StatusName ?? "Unknown",
            UserId = e.UserId,
            BookingId = e.BookingId,
            EventId = e.EventId,
            Items = e.InvoiceItems.Select(i => new Domain.Models.InvoiceItem
            {
                Id = i.Id,
                TicketCategory = i.TicketCategory,
                Price = i.Price,
                Quantity = i.Quantity
            })
            .ToList()
        };
    }
}
