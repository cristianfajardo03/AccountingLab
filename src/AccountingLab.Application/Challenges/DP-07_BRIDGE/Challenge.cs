using System.Globalization;
using System.Text.Json;
using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP07Bridge;

public sealed record ReportPresentationRequest(string ReportType, string Format, decimal Amount);
public sealed record RenderedFinancialReport(string ReportType, string Format, string ContentType, string Content);
public interface IFinancialRepresentationChallenge{RenderedFinancialReport Render(ReportPresentationRequest request); }
public interface IReportRenderer
{
    string ContentType { get; }

    string Renderizar(string tipoInforme, decimal importe);
}

public sealed class JsonReportRenderer : IReportRenderer
{
    public string ContentType => "application/json";

    public string Renderizar(string tipoInforme, decimal importe)
    {
        var datos = new
        {
            reportType = tipoInforme,
            amount = importe.ToString("F4", CultureInfo.InvariantCulture)
        };

        return JsonSerializer.Serialize(datos);
    }
}

public sealed class CsvReportRenderer : IReportRenderer
{
    public string ContentType => "text/csv";

    public string Renderizar(string tipoInforme, decimal importe)
    {
        return $"reportType,amount\n{tipoInforme},{importe.ToString("F4", CultureInfo.InvariantCulture)}";
    }
}

public abstract class FinancialReport
{
    protected readonly IReportRenderer Representador;

    protected FinancialReport(IReportRenderer representador)
    {
        Representador = representador;
    }

    public abstract string Generar(decimal importe);
}

public sealed class BalanceReport : FinancialReport
{
    public BalanceReport(IReportRenderer representador)
        : base(representador)
    {
    }

    public override string Generar(decimal importe)
    {
        return Representador.Renderizar("balance", importe);
    }
}

public sealed class ResultsReport : FinancialReport
{
    public ResultsReport(IReportRenderer representador)
        : base(representador)
    {
    }

    public override string Generar(decimal importe)
    {
        return Representador.Renderizar("resultados", importe);
    }
}

public sealed class FinancialRepresentationChallenge
    : IFinancialRepresentationChallenge
{
    public RenderedFinancialReport Render(ReportPresentationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReportRenderer representador = request.Format switch
        {
            "json" => new JsonReportRenderer(),
            "csv" => new CsvReportRenderer(),

            _ => throw new ArgumentException(
                $"El formato '{request.Format}' no está disponible.",
                nameof(request)
            )
        };

        FinancialReport informe = request.ReportType switch
        {
            "balance" => new BalanceReport(representador),
            "resultados" => new ResultsReport(representador),

            _ => throw new ArgumentException(
                $"El tipo de informe '{request.ReportType}' no está soportado.",
                nameof(request)
            )
        };

        string contenido = informe.Generar(request.Amount);

        return new RenderedFinancialReport(
            request.ReportType,
            request.Format,
            representador.ContentType,
            contenido
        );
    }
}
