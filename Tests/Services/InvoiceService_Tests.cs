using Business;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Domain.Responses;
using Grpc.Core;
using Moq;
using System.Linq.Expressions;
using System.Threading.Channels;

namespace Tests.Services;

public class InvoiceService_Tests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IInvoiceStatusRepository> _invoiceStatusRepositoryMock;
    private readonly Mock<IUpdateBookingWithInvoiceIdHandler> _updateBookingWithInvoiceIdHandlerMock;

    private readonly Mock<BookingManager.BookingManagerClient> _bookingManagerClientMock;
    private readonly Mock<EventContract.EventContractClient> _eventContractClientMock;
    private readonly Mock<AccountGrpcService.AccountGrpcServiceClient> _accountGrpcServiceClientMock;

    private readonly InvoiceService _invoiceService;

    public InvoiceService_Tests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _invoiceStatusRepositoryMock = new Mock<IInvoiceStatusRepository>();
        _updateBookingWithInvoiceIdHandlerMock = new Mock<IUpdateBookingWithInvoiceIdHandler>();

        var dummyInvoker = new Mock<CallInvoker>().Object;

        _bookingManagerClientMock = new Mock<BookingManager.BookingManagerClient>(MockBehavior.Strict, dummyInvoker);
        _eventContractClientMock = new Mock<EventContract.EventContractClient>(MockBehavior.Strict, dummyInvoker);
        _accountGrpcServiceClientMock = new Mock<AccountGrpcService.AccountGrpcServiceClient>(MockBehavior.Strict, dummyInvoker);

        _invoiceService = new InvoiceService(
            _invoiceRepositoryMock.Object,
            _invoiceStatusRepositoryMock.Object,
            _updateBookingWithInvoiceIdHandlerMock.Object,
            _bookingManagerClientMock.Object,
            _eventContractClientMock.Object,
            _accountGrpcServiceClientMock.Object
        );
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldReturnBadRequest_WhenInvoiceIsNull()
    {
        // Act
        var result = await _invoiceService.CreateInvoiceAsync(null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid invoice form.", result.Error);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldNotPublishAndReturnError_WhenAddAsyncFails()
    {
        // Arrange
        var form = new CreateInvoicePayload
        {
            BookingId = "b1",
            EventId = "e1",
            UserId = "u1",
            TicketCategoryName = "Gold",
            TicketQuantity = 2,
            TicketPrice = 100.00m
        };

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<InvoiceEntity>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = false,
                  StatusCode = 500,
                  Error = "Object reference not set to an instance of an object."
              });

        // Act
        var result = await _invoiceService.CreateInvoiceAsync(form);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Object reference not set to an instance of an object.", result.Error);
        _updateBookingWithInvoiceIdHandlerMock.Verify(
            x => x.PublishAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldPublishAndReturnSuccess_WhenAddAsyncSucceeds()
    {
        // Arrange
        var form = new CreateInvoicePayload
        {
            BookingId = "b1",
            EventId = "e1",
            UserId = "u1",
            TicketCategoryName = "Gold",
            TicketQuantity = 2,
            TicketPrice = 100.00m
        };

        _invoiceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<InvoiceEntity>()))
            .ReturnsAsync(new RepositoryResult<Invoice>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = new Invoice { Id = "inv1" }
            });

        var fakeBookingReply = new GetOneBookingReply
        {
            Succeeded = true,
            Booking = new Booking
            {
                Id = form.BookingId,
                EventId = form.EventId,
                UserId = form.UserId,
                InvoiceId = "inv1",
                StatusId = 1,
                TicketCategoryName = form.TicketCategoryName,
                TicketQuantity = form.TicketQuantity,
                TicketPrice = form.TicketPrice.ToString()
            }
        };
        _bookingManagerClientMock
            .Setup(c => c.GetOneBookingAsync(
              It.IsAny<GetOneBookingRequest>(),
              It.IsAny<CallOptions>()))
            .Returns(
            new AsyncUnaryCall<GetOneBookingReply>(
                Task.FromResult(fakeBookingReply),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }
            )
        );

        var fakeEventReply = new GetEventByIdReply
        {
            Event = new Event
            {
                EventId = form.EventId,
                EventName = "Coldplay",
                EventCategoryName = "Music",
                EventDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                EventTime = DateTime.UtcNow.ToString("HH:mm:ss")
            }
        };
        _eventContractClientMock
            .Setup(c => c.GetEventByIdAsync(
              It.IsAny<GetEventByIdRequest>(),
              It.IsAny<CallOptions>()))
            .Returns(
            new AsyncUnaryCall<GetEventByIdReply>(
                Task.FromResult(fakeEventReply),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }
            )
        );

        var fakeAccountReply = new GetAccountByIdReply
        {
            Account = new Account
            {
                UserId = form.UserId,
                UserName = "Test",
                Email = "test@domain.com",
                PhoneNumber = "0701234567"
            }
        };
        _accountGrpcServiceClientMock
            .Setup(c => c.GetAccountByIdAsync(
              It.IsAny<GetAccountByIdRequest>(),
              It.IsAny<CallOptions>()))
            .Returns(
            new AsyncUnaryCall<GetAccountByIdReply>(
                Task.FromResult(fakeAccountReply),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }
            )
        );

        // Act
        var result = await _invoiceService.CreateInvoiceAsync(form);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("inv1", result.Result!.Id);
        _updateBookingWithInvoiceIdHandlerMock.Verify(
            x => x.PublishAsync(form.BookingId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnError_WhenRepositoryFails()
    {
        // Arrange
        _invoiceRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<Expression<Func<InvoiceEntity, object>>>(),
                  null, 0, It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
            .ReturnsAsync(new RepositoryResult<IEnumerable<Invoice>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = "Failed to retrieve invoices."
            });

        // Act
        var result = await _invoiceService.GetAllAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Failed to retrieve invoices.", result.Error);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnInvoices_WhenRepositorySucceeds()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            new Invoice { Id = "1" },
            new Invoice { Id = "2" }
        };

        _invoiceRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<Expression<Func<InvoiceEntity, object>>>(),
                  null, 0, It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
            .ReturnsAsync(new RepositoryResult<IEnumerable<Invoice>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = invoices
            });

        // Act
        var result = await _invoiceService.GetAllAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Result!.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenInvoiceDoesNotExist()
    {
        // Arrange
        _invoiceRepositoryMock
            .Setup(x => x.GetAsync(
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = false,
                  StatusCode = 404,
                  Error = "not found"
              });

        // Act
        var result = await _invoiceService.GetByIdAsync("false-id");

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("not found", result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnInvoice_WhenExists()
    {
        // Arrange
        var invoice = new Invoice { Id = "1" };
        _invoiceRepositoryMock
            .Setup(x => x.GetAsync(
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = true,
                  StatusCode = 200,
                  Result = invoice
              });

        // Act
        var result = await _invoiceService.GetByIdAsync("1");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("1", result.Result!.Id);
    }

    [Fact]
    public async Task GetByStatusIdAsync_ShouldReturnError_WhenRepositoryFails()
    {
        // Arrange
        _invoiceRepositoryMock
            .Setup(x => x.GetAllAsync(
                  true,
                  It.IsAny<Expression<Func<InvoiceEntity, object>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  0, It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<IEnumerable<Invoice>>
              {
                  Succeeded = false,
                  StatusCode = 500,
                  Error = "Failed to retrieve invoices by status id"
              });

        // Act
        var result = await _invoiceService.GetByStatusIdAsync(5);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Failed to retrieve invoices by status id", result.Error);
    }

    [Fact]
    public async Task GetByStatusIdAsync_ShouldReturnInvoices_WhenRepositorySucceeds()
    {
        // Arrange
        var data = new List<Invoice>
        {
            new Invoice { Id = "1" },
            new Invoice { Id = "2" }
        };

        _invoiceRepositoryMock
            .Setup(x => x.GetAllAsync(
                  true,
                  It.IsAny<Expression<Func<InvoiceEntity, object>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  0, It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<IEnumerable<Invoice>>
              {
                  Succeeded = true,
                  StatusCode = 200,
                  Result = data
              });

        // Act
        var result = await _invoiceService.GetByStatusIdAsync(1);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Result!.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenFormIsNull()
    {
        // Act
        var result = await _invoiceService.UpdateAsync(null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenInvoiceDoesNotExist()
    {
        // Arrange
        _invoiceRepositoryMock
            .Setup(x => x.GetAsync(
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = false,
                  StatusCode = 404,
                  Error = $"Invoice with id 'false-id' not found."
              });

        // Act
        var result = await _invoiceService.UpdateAsync(new UpdateInvoiceFormData { Id = "false-id" });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Invoice with id 'false-id' not found.", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateInvoice_WhenRepositorySucceeds()
    {
        // Arrange
        var existingInvoice = new Invoice { Id = "1" };
        _invoiceRepositoryMock
            .Setup(x => x.GetAsync(
                  It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                  It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = true,
                  StatusCode = 200,
                  Result = existingInvoice
              });

        _invoiceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<InvoiceEntity>()))
              .ReturnsAsync(new RepositoryResult<Invoice>
              {
                  Succeeded = true,
                  StatusCode = 200,
                  Result = existingInvoice
              });

        var form = new UpdateInvoiceFormData { Id = "1", Items = new List<InvoiceItemUpdateFormData>() };

        // Act
        var result = await _invoiceService.UpdateAsync(form);

        // Assert
        Assert.True(result.Succeeded);
        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<InvoiceEntity>()), Times.Once);
    }

    private void SetupForStatusChange()
    {
        _invoiceRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<InvoiceEntity, bool>>>(),
                It.IsAny<Expression<Func<InvoiceEntity, object>>[]>()))
            .ReturnsAsync(new RepositoryResult<Invoice>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = new Invoice { Id = "s1", InvoiceStatusId = 1 }
            });

        _invoiceStatusRepositoryMock
            .Setup(r => r.GetAllAsync(
              It.IsAny<bool>(),
              It.IsAny<Expression<Func<InvoiceStatusEntity, object>>>(),
              It.IsAny<Expression<Func<InvoiceStatusEntity, bool>>>(),
              It.IsAny<int>(),
              It.IsAny<Expression<Func<InvoiceStatusEntity, object>>[]>()
            ))
            .ReturnsAsync(new RepositoryResult<IEnumerable<InvoiceStatus>>
            {
                Succeeded = true,
                StatusCode = 200,
                Result = new[]
                {
                    new InvoiceStatus { Id = 1, StatusName = "Unpaid" },
                    new InvoiceStatus { Id = 2, StatusName = "Paid" },
                    new InvoiceStatus { Id = 3, StatusName = "Held" },
                    new InvoiceStatus { Id = 4, StatusName = "Sent" }
                }
        });

        _invoiceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<InvoiceEntity>()))
            .ReturnsAsync(new RepositoryResult<Invoice> { Succeeded = true, StatusCode = 200 });
    }

    [Theory]
    [InlineData("Unpaid", 1)]
    [InlineData("Paid", 2)]
    [InlineData("Held", 3)]
    [InlineData("Sent", 4)]
    public async Task ChangeStatusMethods_ShouldChangeStatus_WhenValidStatusIsProvided(string statusName, int expectedStatusId)
    {
        // Arrange
        SetupForStatusChange();

        // Act
        InvoiceResult result = statusName switch
        {
            "Unpaid" => await _invoiceService.MarkAsUnpaidAsync("s1"),
            "Paid" => await _invoiceService.MarkAsPaidAsync("s1"),
            "Held" => await _invoiceService.HoldAsync("s1"),
            "Sent" => await _invoiceService.SendAsync("s1"),
            _ => throw new ArgumentOutOfRangeException()
        };

        // Assert
        Assert.True(result.Succeeded);
        _invoiceRepositoryMock.Verify(x => x.UpdateAsync(It.Is<InvoiceEntity>(e => e.InvoiceStatusId == expectedStatusId)), Times.Once);
    }
}
