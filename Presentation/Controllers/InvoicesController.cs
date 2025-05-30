using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions.Attributes;

namespace Presentation.Controllers;

[UseApiKey]
[Route("api/[controller]")]
[ApiController]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;

    [HttpGet]
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
