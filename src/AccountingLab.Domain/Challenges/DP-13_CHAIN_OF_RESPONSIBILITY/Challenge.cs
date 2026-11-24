using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP13ChainOfResponsibility;

public sealed record PaymentApprovalRequest(decimal Amount, decimal AvailableFunds, bool PolicyAllows, decimal AuthorizationLimit, IReadOnlyList<string> Checks);
public sealed record PaymentApprovalDecision(bool Approved, string? RejectedBy, IReadOnlyList<string> VisitedChecks);
public interface IPaymentApprovalChallenge{PaymentApprovalDecision Evaluate(PaymentApprovalRequest request); }
public interface IPaymentCheckHandler
{
    string Name { get; }

    bool IsRejected(PaymentApprovalRequest request);
}

public sealed class PolicyCheckHandler : IPaymentCheckHandler
{
    public string Name => "politica";

    public bool IsRejected(PaymentApprovalRequest request)
    {
        return !request.PolicyAllows;
    }
}

public sealed class FundsCheckHandler : IPaymentCheckHandler
{
    public string Name => "fondos";

    public bool IsRejected(PaymentApprovalRequest request)
    {
        return request.Amount > request.AvailableFunds;
    }
}

public sealed class LimitCheckHandler : IPaymentCheckHandler
{
    public string Name => "limite";

    public bool IsRejected(PaymentApprovalRequest request)
    {
        return request.Amount > request.AuthorizationLimit;
    }
}

public sealed class PaymentApprovalChallenge : IPaymentApprovalChallenge
{
    private readonly IReadOnlyDictionary<string, IPaymentCheckHandler> _responsables =
        new Dictionary<string, IPaymentCheckHandler>
        {
            ["politica"] = new PolicyCheckHandler(),
            ["fondos"] = new FundsCheckHandler(),
            ["limite"] = new LimitCheckHandler()
        };

    public PaymentApprovalDecision Evaluate(PaymentApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Checks);

        var validacionesVisitadas = new List<string>();

        foreach (var nombreValidacion in request.Checks)
        {
            if (!_responsables.TryGetValue(nombreValidacion, out var responsable))
            {
                throw new ArgumentException(
                    $"La validación '{nombreValidacion}' no está soportada.",
                    nameof(request)
                );
            }

            validacionesVisitadas.Add(nombreValidacion);

            if (responsable.IsRejected(request))
            {
                return new PaymentApprovalDecision(
                    false,
                    nombreValidacion,
                    validacionesVisitadas.ToArray()
                );
            }
        }

        return new PaymentApprovalDecision(
            true,
            null,
            validacionesVisitadas.ToArray()
        );
    }
}


