using Business.Interfaces;
using Business.Models;
using Grpc.Core;

namespace Presentation.Services;

public class InvoiceGrpcService(IInvoiceService invoiceService) : InvoiceManager.InvoiceManagerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    public override async Task<CreateInvoiceReply> CreateInvoice(CreateInvoiceRequest request, ServerCallContext context)
    {
        try
        {
            var payload = new CreateInvoicePayload
            {
                BookingId = request.BookingId,
                UserId = request.UserId,
                EventId = request.EventId,
                TicketQuantity = request.TicketQuantity,
                TicketPrice = (decimal)request.TicketPrice,
                TicketCategoryName = request.TicketCategoryName
            };

            var result = await _invoiceService.CreateInvoiceAsync(payload);

            if (!result.Succeeded)
                return new CreateInvoiceReply { Succeeded = false, Message = result.Error ?? "Unknown error." };

            var invoice = result.Result!;
            var proto = new Invoice
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                IssuedDate = invoice.IssuedDate.ToString(),
                DueDate = invoice.DueDate.ToString(),
                BillFromName = invoice.BillFromName,
                BillFromAddress = invoice.BillFromAddress,
                BillFromEmail = invoice.BillFromEmail,
                BillFromPhone = invoice.BillFromPhone,
                BillToName = invoice.BillToName,
                BillToAddress = invoice.BillToAddress,
                BillToEmail = invoice.BillToEmail,
                BillToPhone = invoice.BillToPhone,
                Status = invoice.InvoiceStatus,
                TicketDetails =
                {
                    invoice.Items.Select(item => new TicketDetail // TODO: Kolla upp om det är överflödigt med ticket details
                    {
                        TicketCategory = item.TicketCategory,
                        Price = (double)item.Price,
                        Quantity = item.Quantity,
                        Amount = (double)item.Amount
                    })
                },
                UserId = invoice.UserId,
                BookingId = invoice.BookingId,
                EventId = invoice.EventId,
                Total = (double)invoice.Total
            };

            return new CreateInvoiceReply { Succeeded = true, Message = "Invoice created successfully.", Invoice = proto };
        }
        catch (Exception ex)
        {
            return new CreateInvoiceReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<GetInvoicesReply> GetInvoices(GetInvoicesRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _invoiceService.GetAllAsync();
            if (!result.Succeeded)
                return new GetInvoicesReply { Succeeded = false, Message = result.Error ?? "" };

            var invoices = result.Result!.Select(i => new Invoice
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                IssuedDate = i.IssuedDate.ToString("yyyy-MM-dd"),
                DueDate = i.DueDate.ToString("yyyy-MM-dd"),
                BillFromName = i.BillFromName,
                BillFromAddress = i.BillFromAddress,
                BillFromEmail = i.BillFromEmail,
                BillFromPhone = i.BillFromPhone,
                BillToName = i.BillToName,
                BillToAddress = i.BillToAddress,
                BillToEmail = i.BillToEmail,
                BillToPhone = i.BillToPhone,
                TicketDetails =
                {
                    i.Items.Select(item => new TicketDetail
                    {
                        TicketCategory = item.TicketCategory,
                        Price = (double)item.Price,
                        Quantity = item.Quantity,
                        Amount = (double)item.Amount
                    })
                },
                Total = (double)i.Total,
                Status = i.InvoiceStatus,
                UserId = i.UserId,
                BookingId = i.BookingId,
                EventId = i.EventId
            });

            return new GetInvoicesReply { Succeeded = true, Invoices = { invoices } };
        }
        catch (Exception ex)
        {
            return new GetInvoicesReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<GetInvoiceByIdReply> GetInvoiceById(GetInvoiceByIdRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.InvoiceId))
                return new GetInvoiceByIdReply { Succeeded = false, Message = "Invalid invoice id." };

            var result = await _invoiceService.GetByIdAsync(request.InvoiceId);
            if (!result.Succeeded)
                return new GetInvoiceByIdReply { Succeeded = false, Message = result.Error ?? "" };

            var invoice = result.Result!;
            var invoiceReply = new Invoice
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                IssuedDate = invoice.IssuedDate.ToString("yyyy-MM-dd"),
                DueDate = invoice.DueDate.ToString("yyyy-MM-dd"),
                BillFromName = invoice.BillFromName,
                BillFromAddress = invoice.BillFromAddress,
                BillFromEmail = invoice.BillFromEmail,
                BillFromPhone = invoice.BillFromPhone,
                BillToName = invoice.BillToName,
                BillToAddress = invoice.BillToAddress,
                BillToEmail = invoice.BillToEmail,
                BillToPhone = invoice.BillToPhone,
                TicketDetails =
                {
                    invoice.Items.Select(item => new TicketDetail
                    {
                        TicketCategory = item.TicketCategory,
                        Price = (double)item.Price,
                        Quantity = item.Quantity,
                        Amount = (double)item.Amount
                    })
                },
                Total = (double)invoice.Total,
                Status = invoice.InvoiceStatus,
                UserId = invoice.UserId,
                BookingId = invoice.BookingId,
                EventId = invoice.EventId
            };

            return new GetInvoiceByIdReply { Succeeded = true, Invoice = invoiceReply };
        }
        catch (Exception ex)
        {
            return new GetInvoiceByIdReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<UpdateInvoiceByIdReply> UpdateInvoiceById(UpdateInvoiceByIdRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.InvoiceId))
                return new UpdateInvoiceByIdReply { Succeeded = false, Message = "Invalid invoice id." };

            var invoice = request.Invoice;
            var formData = new UpdateInvoiceFormData
            {
                Id = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                IssuedDate = DateTime.Parse(invoice.IssuedDate),
                DueDate = DateTime.Parse(invoice.DueDate),
                BillFromName = invoice.BillFromName,
                BillFromAddress = invoice.BillFromAddress,
                BillFromEmail = invoice.BillFromEmail,
                BillFromPhone = invoice.BillFromPhone,
                BillToName = invoice.BillToName,
                BillToAddress = invoice.BillToAddress,
                BillToEmail = invoice.BillToEmail,
                BillToPhone = invoice.BillToPhone,
                Items = [.. invoice.TicketDetails.Select(item => new InvoiceItemUpdateFormData
                {
                    Id = string.Empty,
                    TicketCategory = item.TicketCategory,
                    Price = (decimal)item.Price,
                    Quantity = item.Quantity
                })],
                UserId = invoice.UserId,
                BookingId = invoice.BookingId,
                EventId = invoice.EventId
            };

            var result = await _invoiceService.UpdateAsync(formData);
            return new UpdateInvoiceByIdReply { Succeeded = result.Succeeded, Message = result.Error ?? "" };
        }
        catch (Exception ex)
        {
            return new UpdateInvoiceByIdReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<SendInvoiceReply> SendInvoice(SendInvoiceRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _invoiceService.SendAsync(request.InvoiceId);
            return new SendInvoiceReply { Succeeded = result.Succeeded, Message = result.Error ?? "" };
        }
        catch (Exception ex)
        {
            return new SendInvoiceReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<HoldInvoiceReply> HoldInvoice(HoldInvoiceRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _invoiceService.HoldAsync(request.InvoiceId);
            return new HoldInvoiceReply { Succeeded = result.Succeeded, Message = result.Error ?? "" };
        }
        catch (Exception ex)
        {
            return new HoldInvoiceReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<UpdateInvoiceStatusReply> MarkInvoicePaid(UpdateInvoiceStatusRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _invoiceService.MarkAsPaidAsync(request.InvoiceId);
            return new UpdateInvoiceStatusReply { Succeeded = result.Succeeded, Message = result.Error ?? "" };
        }
        catch (Exception ex)
        {
            return new UpdateInvoiceStatusReply { Succeeded = false, Message = ex.Message };
        }
    }

    public override async Task<UpdateInvoiceStatusReply> MarkInvoiceUnpaid(UpdateInvoiceStatusRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _invoiceService.MarkAsUnpaidAsync(request.InvoiceId);
            return new UpdateInvoiceStatusReply { Succeeded = result.Succeeded, Message = result.Error ?? "" };
        }
        catch (Exception ex)
        {
            return new UpdateInvoiceStatusReply { Succeeded = false, Message = ex.Message };
        }
    }
}
