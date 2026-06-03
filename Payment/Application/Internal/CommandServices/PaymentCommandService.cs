using Moveo_backend.Payment.Domain.Model.Commands;
using Moveo_backend.Payment.Domain.Repositories;
using Moveo_backend.Payment.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;

namespace Moveo_backend.Payment.Application.Internal.CommandServices;

public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork) : IPaymentCommandService
{
    public async Task<PaymentEntity?> Handle(CreatePaymentCommand command)
    {
        var payment = new PaymentEntity(command);
        await paymentRepository.AddAsync(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }

    public async Task<PaymentEntity?> Handle(UpdatePaymentCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.Id);
        if (payment is null) return null;

        payment.Update(command);
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }

    public async Task<PaymentEntity?> Handle(PatchPaymentCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.Id);
        if (payment is null) return null;

        payment.PartialUpdate(command);
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }

    public async Task<bool> Handle(DeletePaymentCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.Id);
        if (payment is null) return false;

        paymentRepository.Remove(payment);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
