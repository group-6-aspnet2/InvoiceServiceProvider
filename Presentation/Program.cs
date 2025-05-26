using Azure.Messaging.ServiceBus;
using Business;
using Business.Interfaces;
using Business.Services;
using Data.Contexts;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddSwaggerGen();


//builder.WebHost.ConfigureKestrel(x =>
//{
//    x.ListenAnyIP(8585, listenOption =>
//    {
//        listenOption.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
//    });

//    x.ListenAnyIP(7239, listenOption =>
//    {
//        listenOption.UseHttps();
//        listenOption.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
//    });
//});

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceStatusRepository, InvoiceStatusRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new ServiceBusClient(configuration["AzureServiceBusSettings:ConnectionString"]);
});

builder.Services.AddHostedService<InvoiceQueueBackgroundService>();
builder.Services.AddScoped<IUpdateBookingWithInvoiceIdHandler, UpdateBookingWithInvoiceIdHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice Service API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/grpc", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.MapGrpcService<InvoiceGrpcService>();
app.MapOpenApi();

app.Run();
