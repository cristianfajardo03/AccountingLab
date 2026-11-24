using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP21TemplateMethod;

public sealed record BankStatementItem(string Id, string Reference, DateOnly Date, decimal Amount);
public sealed record ReconciliationLedgerItem(string Id, string Reference, DateOnly Date, decimal Amount);
public sealed record BankReconciliationRequest(string Mode, IReadOnlyList<BankStatementItem> BankItems, IReadOnlyList<ReconciliationLedgerItem> LedgerItems, decimal Tolerance);
public sealed record ReconciliationMatch(string BankItemId, string LedgerItemId, decimal Difference);
public sealed record BankReconciliationResult(IReadOnlyList<ReconciliationMatch> Matches, IReadOnlyList<string> UnmatchedBankItems, IReadOnlyList<string> UnmatchedLedgerItems);
public interface IBankReconciliationChallenge{BankReconciliationResult Reconcile(BankReconciliationRequest request); }

internal abstract class ReconciliationTemplate
{
    public BankReconciliationResult Execute(BankReconciliationRequest request)
    {
        var matches = new List<ReconciliationMatch>();
        var usedLedgerItems = new HashSet<int>();
        var matchedBankItems = new HashSet<int>();

        for (int bankIndex = 0; bankIndex < request.BankItems.Count; bankIndex++)
        {
            var bankItem = request.BankItems[bankIndex];

            for (int ledgerIndex = 0;
                 ledgerIndex < request.LedgerItems.Count;
                 ledgerIndex++)
            {
                if (usedLedgerItems.Contains(ledgerIndex))
                    continue;

                var ledgerItem = request.LedgerItems[ledgerIndex];

                if (!HasSameReferenceAndDate(bankItem, ledgerItem))
                    continue;

                var difference = Math.Abs(
                    bankItem.Amount - ledgerItem.Amount);

                if (!IsAmountMatch(difference, request.Tolerance))
                    continue;

                matches.Add(new ReconciliationMatch(
                    bankItem.Id,
                    ledgerItem.Id,
                    difference));

                usedLedgerItems.Add(ledgerIndex);
                matchedBankItems.Add(bankIndex);

                break;
            }
        }

        var unmatchedBankItems = request.BankItems
            .Where((item, index) => !matchedBankItems.Contains(index))
            .Select(item => item.Id)
            .ToList();

        var unmatchedLedgerItems = request.LedgerItems
            .Where((item, index) => !usedLedgerItems.Contains(index))
            .Select(item => item.Id)
            .ToList();

        return new BankReconciliationResult(
            matches,
            unmatchedBankItems,
            unmatchedLedgerItems);
    }

    private static bool HasSameReferenceAndDate(
        BankStatementItem bankItem,
        ReconciliationLedgerItem ledgerItem)
    {
        return string.Equals(
                   bankItem.Reference,
                   ledgerItem.Reference,
                   StringComparison.OrdinalIgnoreCase)
               && bankItem.Date == ledgerItem.Date;
    }

    protected abstract bool IsAmountMatch(
        decimal difference,
        decimal tolerance);
}

internal sealed class ExactReconciliation : ReconciliationTemplate
{
    protected override bool IsAmountMatch(
        decimal difference,
        decimal tolerance)
    {
        return difference == 0m;
    }
}

internal sealed class ToleranceReconciliation : ReconciliationTemplate
{
    protected override bool IsAmountMatch(
        decimal difference,
        decimal tolerance)
    {
        return difference <= tolerance;
    }
}

public sealed class BankReconciliationChallenge : IBankReconciliationChallenge
{
    public BankReconciliationResult Reconcile(
        BankReconciliationRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.BankItems is null)
            throw new ArgumentNullException(nameof(request.BankItems));

        if (request.LedgerItems is null)
            throw new ArgumentNullException(nameof(request.LedgerItems));

        if (request.Tolerance < 0)
            throw new ArgumentOutOfRangeException(
                nameof(request.Tolerance),
                "La tolerancia no puede ser negativa.");

        ReconciliationTemplate template = request.Mode?.Trim().ToLowerInvariant() switch
        {
            "exact" => new ExactReconciliation(),

            "tolerance" => new ToleranceReconciliation(),

            _ => throw new InvalidOperationException(
                $"Modo de conciliación no soportado: {request.Mode}.")
        };

        return template.Execute(request);
    }
}

