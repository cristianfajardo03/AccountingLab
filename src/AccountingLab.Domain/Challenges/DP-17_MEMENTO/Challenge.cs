using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP17Memento;

public sealed class BudgetDraftLine
{
    public string AccountCode { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class BudgetDraft
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool Approved { get; set; }
    public List<BudgetDraftLine> Lines { get; set; } = new();
}

public interface IBudgetSnapshot { }

public interface IBudgetHistoryChallenge
{
    IBudgetSnapshot Save(BudgetDraft draft);
    BudgetDraft Restore(BudgetDraft current, IBudgetSnapshot snapshot);
}

internal sealed class BudgetSnapshot : IBudgetSnapshot
{
    public Guid Id { get; }
    public string Name { get; }
    public bool Approved { get; }
    public IReadOnlyList<BudgetDraftLine> Lines { get; }

    public BudgetSnapshot(BudgetDraft draft)
    {
        Id = draft.Id;
        Name = draft.Name;
        Approved = draft.Approved;

        Lines = draft.Lines
            .Select(line => new BudgetDraftLine
            {
                AccountCode = line.AccountCode,
                Amount = line.Amount
            })
            .ToList();
    }
}

public sealed class BudgetHistoryChallenge : IBudgetHistoryChallenge
{
    public IBudgetSnapshot Save(BudgetDraft draft)
    {
        if (draft is null)
            throw new ArgumentNullException(nameof(draft));

        if (draft.Approved)
            throw new InvalidOperationException(
                "No se puede crear un snapshot de un presupuesto aprobado.");

        return new BudgetSnapshot(draft);
    }

    public BudgetDraft Restore(BudgetDraft current, IBudgetSnapshot snapshot)
    {
        if (current is null)
            throw new ArgumentNullException(nameof(current));

        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        if (snapshot is not BudgetSnapshot savedState)
            throw new ArgumentException(
                "El snapshot no pertenece a este mecanismo de historial.",
                nameof(snapshot));

        return new BudgetDraft
        {
            Id = savedState.Id,
            Name = savedState.Name,
            Approved = savedState.Approved,
            Lines = savedState.Lines
                .Select(line => new BudgetDraftLine
                {
                    AccountCode = line.AccountCode,
                    Amount = line.Amount
                })
                .ToList()
        };
    }
}

