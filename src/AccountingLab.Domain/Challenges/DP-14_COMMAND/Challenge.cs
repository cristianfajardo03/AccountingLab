using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP14Command;

public sealed record AccountingOperationRequest(Guid CompanyId, Guid OperationId, string Kind, Guid TargetId, decimal Amount);
public sealed record AccountingOperationReceipt(Guid OperationId, Guid TargetId, string Kind, decimal AppliedAmount, decimal RemainingAmount, Guid LinkedJournalEntryId, bool Replayed);
public sealed record AccountingOperationContext(decimal OutstandingAmount, bool TargetIsPublished, IReadOnlyList<AccountingOperationReceipt> CompletedOperations);
public interface IIdentifiedOperationChallenge
{
    AccountingOperationReceipt Execute(
        AccountingOperationRequest request,
        AccountingOperationContext context
    );
}

public interface IAccountingCommand
{
    AccountingOperationReceipt Execute(
        AccountingOperationRequest request,
        AccountingOperationContext context
    );
}

public sealed class ApplyPaymentCommand : IAccountingCommand
{
    public AccountingOperationReceipt Execute(
        AccountingOperationRequest request,
        AccountingOperationContext context
    )
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException(
                "El importe del pago debe ser mayor que cero."
            );
        }

        if (request.Amount > context.OutstandingAmount)
        {
            throw new InvalidOperationException(
                "El pago supera el importe pendiente."
            );
        }

        decimal restante = context.OutstandingAmount - request.Amount;

        return new AccountingOperationReceipt(
            request.OperationId,
            request.TargetId,
            request.Kind,
            request.Amount,
            restante,
            Guid.NewGuid(),
            false
        );
    }
}

public sealed class ReverseVoucherCommand : IAccountingCommand
{
    public AccountingOperationReceipt Execute(
        AccountingOperationRequest request,
        AccountingOperationContext context
    )
    {
        if (!context.TargetIsPublished)
        {
            throw new InvalidOperationException(
                "No se puede reversar un comprobante que no ha sido publicado."
            );
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException(
                "El importe de reversión debe ser mayor que cero."
            );
        }

        return new AccountingOperationReceipt(
            request.OperationId,
            request.TargetId,
            request.Kind,
            request.Amount,
            context.OutstandingAmount,
            Guid.NewGuid(),
            false
        );
    }
}

public sealed class IdentifiedOperationChallenge
    : IIdentifiedOperationChallenge
{
    public AccountingOperationReceipt Execute(
        AccountingOperationRequest request,
        AccountingOperationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.CompletedOperations);

        var operacionAnterior = context.CompletedOperations
            .FirstOrDefault(x => x.OperationId == request.OperationId);

        if (operacionAnterior is not null)
        {
            return operacionAnterior with
            {
                Replayed = true
            };
        }

        IAccountingCommand comando = request.Kind switch
        {
            "apply-payment" => new ApplyPaymentCommand(),

            "reverse-voucher" => new ReverseVoucherCommand(),

            _ => throw new ArgumentException(
                $"La operación '{request.Kind}' no está soportada.",
                nameof(request)
            )
        };

        return comando.Execute(request, context);
    }
}
