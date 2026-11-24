using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP08Composite;

public sealed record AccountHierarchy(string Code, bool AllowsPosting, decimal OwnBalance, IReadOnlyList<AccountHierarchy> Children);
public interface IAccountTreeChallenge{decimal Total(AccountHierarchy account); }
public sealed class AccountTreeChallenge : IAccountTreeChallenge
{
    public decimal Total(AccountHierarchy account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.AllowsPosting && account.Children.Count > 0)
        {
            throw new ArgumentException(
                "Una cuenta que permite movimientos no puede contener cuentas hijas.",
                nameof(account)
            );
        }

        if (account.AllowsPosting)
        {
            return account.OwnBalance;
        }

        decimal total = account.OwnBalance;

        foreach (var cuentaHija in account.Children)
        {
            total += Total(cuentaHija);
        }

        return total;
    }
}

