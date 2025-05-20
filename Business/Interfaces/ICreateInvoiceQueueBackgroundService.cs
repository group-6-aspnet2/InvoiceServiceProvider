namespace Business.Interfaces
{
    public interface ICreateInvoiceQueueBackgroundService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}