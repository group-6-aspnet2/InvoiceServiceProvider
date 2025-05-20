namespace Business.Interfaces;

public interface IUpdateBookingWithInvoiceIdHandler
{
    Task PublishAsync(string payload);
}
