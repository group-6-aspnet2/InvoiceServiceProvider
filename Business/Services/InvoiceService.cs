using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Models;
using Domain.Responses;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Business.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository, IInvoiceStatusRepository invoiceStatusRepository)
{
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly IInvoiceStatusRepository _invoiceStatusRepository = invoiceStatusRepository;


    public async Task<InvoiceResult<Invoice>> CreateInvoiceAsync(CreateInvoiceFormData formData)
    {
        try
        {
            if (formData == null)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 400, Error = "Invalid invoice form." };

            var invoiceEntity = new InvoiceEntity
            {
                Id = Guid.NewGuid().ToString(),
                InvoiceNumber = formData.InvoiceNumber,
                IssuedDate = formData.IssuedDate,
                DueDate = formData.DueDate,
                BillFromName = formData.BillFromName,
                BillFromAddress = formData.BillFromAddress,
                BillFromEmail = formData.BillFromEmail,
                BillFromPhone = formData.BillFromPhone,
                BillToName = formData.BillToName,
                BillToAddress = formData.BillToAddress,
                BillToEmail = formData.BillToEmail,
                BillToPhone = formData.BillToPhone,
                InvoiceStatusId = 1,
                InvoiceItems = [.. formData.Items.Select(i => new InvoiceItemEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    TicketCategory = i.TicketCategory,
                    Price = i.Price,
                    Quantity = i.Quantity
                })]
            };

            var result = await _invoiceRepository.AddAsync(invoiceEntity);

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
            var result = await _invoiceRepository.GetAllAsync(
                orderByDescending: true,
                sortBy: i => i.IssuedDate,
                where: null,
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

    public async Task<InvoiceResult<Invoice>> GetByIdAsync(string id)
    {
        try
        {
            var result = await _invoiceRepository.GetAsync(
                where: i => i.Id == id,
                includes: [x => x.InvoiceItems, x => x.InvoiceStatus]
                );

            if (!result.Succeeded)
                return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };

            return new InvoiceResult<Invoice> { Succeeded = true, StatusCode = result.StatusCode, Result = result.Result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new InvoiceResult<Invoice> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
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

}
