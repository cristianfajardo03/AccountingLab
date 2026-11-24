using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP03Builder;

public sealed record FinancialReportRequest(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);
public sealed record FinancialReport(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);
public sealed class FinancialReportBuilder{private string _titulo = string.Empty; private DateOnly _desde; private DateOnly _hasta; private readonly List<string> _secciones = new();private readonly List<string> _anexos = new();

    public FinancialReportBuilder EstablecerTitulo(string titulo)
    {
        _titulo = titulo;
        return this;
    }

    public FinancialReportBuilder EstablecerPeriodo(DateOnly desde, DateOnly hasta)
    {
        _desde = desde;
        _hasta = hasta;
        return this;
    }

    public FinancialReportBuilder AgregarSecciones(IEnumerable<string> secciones)
    {
        _secciones.AddRange(secciones);
        return this;
    }

    public FinancialReportBuilder AgregarAnexos(IEnumerable<string> anexos)
    {
        _anexos.AddRange(anexos);
        return this;
    }

    public FinancialReport Construir()
    {
        return new FinancialReport(
            _titulo,
            _desde,
            _hasta,
            _secciones.ToArray(),
            _anexos.ToArray()
        );
    }
}

public interface IReportAssemblyChallenge
{
    FinancialReport Build(FinancialReportRequest request);
}

public sealed class ReportAssemblyChallenge : IReportAssemblyChallenge
{
    public FinancialReport Build(FinancialReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.From > request.To)
        {
            throw new ArgumentException(
                "La fecha inicial no puede ser posterior a la fecha final.",
                nameof(request)
            );
        }

        ArgumentNullException.ThrowIfNull(request.Sections);
        ArgumentNullException.ThrowIfNull(request.Annexes);

        return new FinancialReportBuilder()
            .EstablecerTitulo(request.Title)
            .EstablecerPeriodo(request.From, request.To)
            .AgregarSecciones(request.Sections)
            .AgregarAnexos(request.Annexes)
            .Construir();
    }
}

