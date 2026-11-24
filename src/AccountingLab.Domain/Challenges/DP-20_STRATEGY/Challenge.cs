using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP20Strategy;

public sealed record FictionalTaxRule(string Name, string OperationKind, DateOnly ValidFrom, DateOnly ValidTo, string CalculationKind, decimal Rate, decimal FixedAmount);
public sealed record TaxCalculationRequest(decimal BaseAmount, string OperationKind, DateOnly AccountingDate, IReadOnlyList<FictionalTaxRule> Rules);
public sealed record TaxCalculationResult(decimal BaseAmount, decimal AppliedRate, decimal TaxAmount, string RuleName);
public interface IConfigurableTaxChallenge{TaxCalculationResult Calculate(TaxCalculationRequest request); }
internal interface ITaxCalculationStrategy
{
    decimal Calculate(decimal baseAmount, FictionalTaxRule rule);
}

internal sealed class PercentageTaxStrategy : ITaxCalculationStrategy
{
    public decimal Calculate(decimal baseAmount, FictionalTaxRule rule)
    {
        return baseAmount * rule.Rate;
    }
}

internal sealed class FixedTaxStrategy : ITaxCalculationStrategy
{
    public decimal Calculate(decimal baseAmount, FictionalTaxRule rule)
    {
        return rule.FixedAmount;
    }
}

public sealed class ConfigurableTaxChallenge : IConfigurableTaxChallenge
{
    private readonly Dictionary<string, ITaxCalculationStrategy> _strategies;

    public ConfigurableTaxChallenge()
    {
        _strategies = new Dictionary<string, ITaxCalculationStrategy>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["percentage"] = new PercentageTaxStrategy(),
            ["fixed"] = new FixedTaxStrategy()
        };
    }

    public TaxCalculationResult Calculate(TaxCalculationRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.BaseAmount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(request.BaseAmount),
                "El valor base no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(request.OperationKind))
            throw new ArgumentException(
                "El tipo de operación es obligatorio.");

        if (request.Rules is null)
            throw new ArgumentNullException(nameof(request.Rules));

        var rule = request.Rules.FirstOrDefault(r =>
            string.Equals(
                r.OperationKind,
                request.OperationKind,
                StringComparison.OrdinalIgnoreCase)
            &&
            request.AccountingDate >= r.ValidFrom
            &&
            request.AccountingDate <= r.ValidTo);

        if (rule is null)
        {
            throw new InvalidOperationException(
                "No existe una regla tributaria válida para la operación y fecha indicadas.");
        }

        if (!_strategies.TryGetValue(rule.CalculationKind, out var strategy))
        {
            throw new InvalidOperationException(
                $"Tipo de cálculo no soportado: {rule.CalculationKind}.");
        }

        var calculatedTax = strategy.Calculate(request.BaseAmount, rule);

        var taxAmount = Math.Round(
            calculatedTax,
            4,
            MidpointRounding.AwayFromZero);

        var appliedRate = string.Equals(
            rule.CalculationKind,
            "percentage",
            StringComparison.OrdinalIgnoreCase)
                ? rule.Rate
                : 0m;

        return new TaxCalculationResult(
            request.BaseAmount,
            appliedRate,
            taxAmount,
            rule.Name);
    }
}

