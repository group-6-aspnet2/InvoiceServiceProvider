using Business.Interfaces;
using Business.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Presentation.Services;

public class InvoiceGrpcService(IInvoiceService invoiceService) : InvoiceManager.InvoiceManagerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    //public override Task<CreateInvoiceReply> CreateInvoice(CreateInvoiceRequest request, ServerCallContext context)
    //{
    //    var formData = new CreateInvoiceFormData
    //    {
    //        InvoiceNumber = request.Invoice.InvoiceNumber,
    //        IssuedDate = DateTime.Parse(request.Invoice.IssuedDate),
    //        DueDate = DateTime.Parse(request.Invoice.DueDate),
    //        BillFromName = request.Invoice.BillFrom,
    //    }
    //}
}
