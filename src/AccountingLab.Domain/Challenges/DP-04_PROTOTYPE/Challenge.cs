using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP04Prototype;

public sealed class TemplateLine{public string AccountCode { get; set; } = ""; public decimal Amount { get; set; } public List<string> Tags { get; set; } = new(); }
public sealed class RecurringTemplate{public Guid Id { get; set; } public string Name { get; set; } = ""; public List<TemplateLine> Lines { get; set; } = new(); }
public interface ITemplateCopyChallenge{RecurringTemplate Copy(RecurringTemplate source); }

public sealed class RecurringTemplatePrototype
{
    public RecurringTemplate Clonar(RecurringTemplate original)
    {
        return new RecurringTemplate
        {
            Id = Guid.NewGuid(),

            Name = original.Name,

            Lines = original.Lines.Select(linea => new TemplateLine
            {
                AccountCode = linea.AccountCode,
                Amount = linea.Amount,

                Tags = new List<string>(linea.Tags)
            }).ToList()
        };
    }
}

public sealed class TemplateCopyChallenge : ITemplateCopyChallenge
{
    public RecurringTemplate Copy(RecurringTemplate source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var prototipo = new RecurringTemplatePrototype();

        return prototipo.Clonar(source);
    }
}
