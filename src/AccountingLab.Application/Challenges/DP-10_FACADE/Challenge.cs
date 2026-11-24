using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP10Facade;

public sealed record ClosingAccountBalance(string AccountCode, decimal Debit, decimal Credit);
public sealed record ClosePeriodRequest(Guid CompanyId, DateOnly From, DateOnly To, IReadOnlyList<ClosingAccountBalance> Balances, int UnpostedDocuments);
public sealed record PeriodClosingSummary(Guid CompanyId, DateOnly From, DateOnly To, bool Closed, decimal TotalDebit, decimal TotalCredit);
public sealed class PendingDocumentValidator
{
    public void Validate(int documentosPendientes)
    {
        if (documentosPendientes > 0)
        {
            throw new InvalidOperationException(
                "No se puede cerrar el período porque existen documentos pendientes."
            );
        }
    }
}

public sealed class AccountingBalanceConsolidator
{
    public (decimal TotalDebit, decimal TotalCredit) Consolidate(
        IReadOnlyList<ClosingAccountBalance> balances
    )
    {
        decimal totalDebit = balances.Sum(x => x.Debit);
        decimal totalCredit = balances.Sum(x => x.Credit);

        return (totalDebit, totalCredit);
    }
}

public sealed class AccountingBalanceValidator
{
    public void Validate(decimal totalDebit, decimal totalCredit)
    {
        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException(
                "No se puede cerrar el período porque los débitos y créditos no coinciden."
            );
        }
    }
}

public interface IPeriodClosingChallenge
{
    PeriodClosingSummary Close(ClosePeriodRequest request);
}

public sealed class PeriodClosingChallenge : IPeriodClosingChallenge
{
    private readonly PendingDocumentValidator _validadorDocumentos = new();
    private readonly AccountingBalanceConsolidator _consolidador = new();
    private readonly AccountingBalanceValidator _validadorSaldos = new();

    public PeriodClosingSummary Close(ClosePeriodRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(request.Balances);

        _validadorDocumentos.Validate(request.UnpostedDocuments);

        var (totalDebit, totalCredit) =
            _consolidador.Consolidate(request.Balances);

        _validadorSaldos.Validate(totalDebit, totalCredit);

        return new PeriodClosingSummary(
            request.CompanyId,
            request.From,
            request.To,
            true,
            totalDebit,
            totalCredit
        );
    }
}
