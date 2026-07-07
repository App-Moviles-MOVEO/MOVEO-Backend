using Moq;
using Moveo_backend.Payment.Application.Internal.CommandServices;
using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Repositories;
using Moveo_backend.Shared.Domain.Repositories;
using Xunit;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.UnitTests.Payment;

public class PaymentCommandServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly PaymentCommandService _sut;

    public PaymentCommandServiceTests()
    {
        _sut = new PaymentCommandService(_paymentRepository.Object, _unitOfWork.Object);
    }

    private static PaymentEntity BuildPayment() => new(new CreatePaymentCommand(
        PayerId: 1,
        RecipientId: 2,
        RentalId: 3,
        Amount: 100m,
        Currency: "PEN",
        Method: "yape",
        Type: "rental"));

    [Fact]
    public async Task Handle_Create_AddsPaymentAndReturnsIt()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            PayerId: 1, RecipientId: 2, RentalId: 3, Amount: 250m,
            Currency: "PEN", Method: "card", Type: "rental");

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(250m, result!.Amount);
        _paymentRepository.Verify(r => r.AddAsync(It.IsAny<PaymentEntity>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Update_ReturnsNull_WhenPaymentDoesNotExist()
    {
        // Arrange
        var command = new UpdatePaymentCommand(
            Id: 99, PayerId: 1, RecipientId: 2, RentalId: 3, Amount: 100m,
            Currency: "PEN", Method: "card", Status: "completed", TransactionId: "tx1",
            Type: "rental", Description: null, Reason: null, DueDate: null, CompletedAt: null);
        _paymentRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync((PaymentEntity?)null);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.Null(result);
        _paymentRepository.Verify(r => r.Update(It.IsAny<PaymentEntity>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Update_UpdatesPayment_WhenPaymentExists()
    {
        // Arrange
        var payment = BuildPayment();
        var command = new UpdatePaymentCommand(
            Id: 1, PayerId: 1, RecipientId: 2, RentalId: 3, Amount: 500m,
            Currency: "PEN", Method: "transfer", Status: "completed", TransactionId: "tx-42",
            Type: "rental", Description: "Pago actualizado", Reason: null, DueDate: null, CompletedAt: null);
        _paymentRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync(payment);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500m, result!.Amount);
        Assert.Equal("completed", result.Status);
        _paymentRepository.Verify(r => r.Update(payment), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Delete_ReturnsFalse_WhenPaymentDoesNotExist()
    {
        // Arrange
        var command = new DeletePaymentCommand(Id: 99);
        _paymentRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync((PaymentEntity?)null);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.False(result);
        _paymentRepository.Verify(r => r.Remove(It.IsAny<PaymentEntity>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Delete_RemovesPaymentAndReturnsTrue_WhenPaymentExists()
    {
        // Arrange
        var payment = BuildPayment();
        var command = new DeletePaymentCommand(Id: 1);
        _paymentRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync(payment);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.True(result);
        _paymentRepository.Verify(r => r.Remove(payment), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
