using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using Presentation.Documentation;
using Presentation.Extensions.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Controllers;

[UseApiKey]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/[controller]")]
[ApiController]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    [HttpGet]
    [SwaggerOperation(Summary = "Returns all created invoices.")]
    [SwaggerResponse(200, "Invoices retrieved successfully.", typeof(IEnumerable<Domain.Models.Invoice>))]
    [SwaggerResponse(500, "An error occurred while retrieving invoices.")]
    [SwaggerResponseExample(200, typeof(GetAllInvoicesResponse_Example))]
    public async Task<IActionResult> GetAllInvoices()
    {
        try
        {
            var result = await _invoiceService.GetAllAsync();
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Returns an invoice by its id.")]
    [SwaggerResponse(200, "Invoice by id retrieved successfully.", typeof(IEnumerable<Domain.Models.Invoice>))]
    [SwaggerResponse(404, "Invoice not found.")]
    [SwaggerResponseExample(200, typeof(GetInvoiceByIdResponse_Example))]
    public async Task<IActionResult> GetInvoiceById(string id)
    {
        try
        {
            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("status/{statusId}")]
    [SwaggerOperation(Summary = "Returns invoices by their status id.")]
    [SwaggerResponse(200, "Invoices by status id retrieved successfully.", typeof(IEnumerable<Domain.Models.Invoice>))]
    [SwaggerResponse(404, "No invoices found for the given status id.")]
    [SwaggerResponseExample(200, typeof(GetInvoicesByStatusResponse_Example))]
    public async Task<IActionResult> GetInvoiceByStatus(int statusId) 
    {
        try
        {
            var result = await _invoiceService.GetByStatusIdAsync(statusId);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new invoice.")]
    [SwaggerResponse(201, "Invoice created successfully.")]
    [SwaggerResponse(400, "Invalid request data.")]
    [SwaggerRequestExample(typeof(CreateInvoicePayload), typeof(CreateInvoicePayload_Example))]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoicePayload formData)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.CreateInvoiceAsync(formData);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Created("Invoice was created successfully.", result.Result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Updates an existing invoice.")]
    [SwaggerResponse(200, "Invoice updated successfully.")]
    [SwaggerResponse(400, "Invalid request data or id mismatch.")]
    [SwaggerRequestExample(typeof(UpdateInvoiceFormData), typeof(UpdateInvoiceFormData_Example))]
    public async Task<IActionResult> UpdateInvoice(string id, [FromBody] UpdateInvoiceFormData formData)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != formData.Id)
                return BadRequest("Id does not match.");

            var result = await _invoiceService.UpdateAsync(formData);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/send")]
    [SwaggerOperation(Summary = "Sends an invoice.")]
    [SwaggerResponse(200, "Invoice sent successfully.")]
    [SwaggerResponse(404, "Invoice not found.")]
    public async Task<IActionResult> SendInvoice(string id)
    {
        try
        {
            var result = await _invoiceService.SendAsync(id);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/hold")]
    [SwaggerOperation(Summary = "Holds an invoice.")]
    [SwaggerResponse(200, "Invoice held successfully.")]
    [SwaggerResponse(404, "Invoice not found.")]
    public async Task<IActionResult> HoldInvoice(string id)
    {
        try
        {
            var result = await _invoiceService.HoldAsync(id);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/paid")]
    [SwaggerOperation(Summary = "Marks an invoice as paid.")]
    [SwaggerResponse(200, "Invoice marked as paid successfully.")]
    [SwaggerResponse(404, "Invoice not found.")]
    public async Task<IActionResult> MarkAsPaid(string id)
    {
        try
        {
            var result = await _invoiceService.MarkAsPaidAsync(id);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{id}/unpaid")]
    [SwaggerOperation(Summary = "Marks an invoice as unpaid.")]
    [SwaggerResponse(200, "Invoice marked as unpaid successfully.")]
    [SwaggerResponse(404, "Invoice not found.")]
    public async Task<IActionResult> MarkAsUnpaid(string id)
    {
        try
        {
            var result = await _invoiceService.MarkAsUnpaidAsync(id);
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, result.Error);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
