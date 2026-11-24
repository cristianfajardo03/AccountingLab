using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP19State;

public sealed record InvoiceLifecycleRequest(string CurrentState, string Action, decimal OutstandingAmount, DateOnly DueDate, DateOnly BusinessDate);
public sealed record InvoiceTransitionResult(string State, bool PaymentAllowed);
public interface IInvoiceLifecycleChallenge{InvoiceTransitionResult Transition(InvoiceLifecycleRequest request); }

public sealed class InvoiceLifecycleChallenge : IInvoiceLifecycleChallenge
{
    public InvoiceTransitionResult Transition(InvoiceLifecycleRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var currentState = request.CurrentState?.Trim().ToLowerInvariant();
        var action = request.Action?.Trim().ToLowerInvariant();

        return currentState switch
        {
            "draft" => HandleDraft(action, request),

            "issued" => HandleIssued(action, request),

            "overdue" => HandleOverdue(action, request),

            "paid" => HandlePaid(action),

            _ => throw new InvalidOperationException(
                $"Estado de factura no reconocido: {request.CurrentState}.")
        };
    }

    private static InvoiceTransitionResult HandleDraft(
        string? action,
        InvoiceLifecycleRequest request)
    {
        if (action == "emit")
        {
            if (request.OutstandingAmount <= 0)
                throw new InvalidOperationException(
                    "No se puede emitir una factura sin saldo pendiente.");

            return new InvoiceTransitionResult("issued", true);
        }

        throw new InvalidOperationException(
            $"La acción '{action}' no está permitida para una factura en borrador.");
    }

    private static InvoiceTransitionResult HandleIssued(
        string? action,
        InvoiceLifecycleRequest request)
    {
        if (action == "pay")
        {
            if (request.OutstandingAmount > 0)
                throw new InvalidOperationException(
                    "La factura todavía tiene un saldo pendiente.");

            return new InvoiceTransitionResult("paid", false);
        }

        if (action == "mark-overdue")
        {
            if (request.BusinessDate > request.DueDate &&
                request.OutstandingAmount > 0)
            {
                return new InvoiceTransitionResult("overdue", true);
            }

            return new InvoiceTransitionResult("issued", true);
        }

        throw new InvalidOperationException(
            $"La acción '{action}' no está permitida para una factura emitida.");
    }

    private static InvoiceTransitionResult HandleOverdue(
        string? action,
        InvoiceLifecycleRequest request)
    {
        if (action == "pay")
        {
            if (request.OutstandingAmount > 0)
                throw new InvalidOperationException(
                    "La factura todavía tiene un saldo pendiente.");

            return new InvoiceTransitionResult("paid", false);
        }

        if (action == "mark-overdue")
            return new InvoiceTransitionResult("overdue", true);

        throw new InvalidOperationException(
            $"La acción '{action}' no está permitida para una factura vencida.");
    }

    private static InvoiceTransitionResult HandlePaid(string? action)
    {
        throw new InvalidOperationException(
            $"No se puede ejecutar la acción '{action}' sobre una factura pagada.");
    }
}

