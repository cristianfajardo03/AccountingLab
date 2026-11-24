using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP12Proxy;

public sealed record SensitiveReportRequest(Guid CompanyId, bool MayRead, string ReportKey);
public sealed record SensitiveFinancialReport(Guid CompanyId, string ReportKey, decimal ConfidentialAmount);
public interface ISensitiveReportChallenge{SensitiveFinancialReport Get(SensitiveReportRequest request,Func<SensitiveFinancialReport> loadReport); }
public sealed class SensitiveReportProxy
{
    public SensitiveFinancialReport Obtener(
        SensitiveReportRequest solicitud,
        Func<SensitiveFinancialReport> cargarInforme
    )
    {
        if (!solicitud.MayRead)
        {
            throw new UnauthorizedAccessException(
                "No tienes permisos para consultar este informe financiero."
            );
        }

        var informe = cargarInforme();

        if (informe.CompanyId != solicitud.CompanyId)
        {
            throw new InvalidOperationException(
                "El informe consultado no pertenece a la empresa solicitada."
            );
        }

        if (informe.ReportKey != solicitud.ReportKey)
        {
            throw new InvalidOperationException(
                "El informe obtenido no corresponde al solicitado."
            );
        }

        return informe;
    }
}

public sealed class SensitiveReportChallenge : ISensitiveReportChallenge
{
    private readonly SensitiveReportProxy _representante = new();

    public SensitiveFinancialReport Get(
        SensitiveReportRequest request,
        Func<SensitiveFinancialReport> loadReport
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(loadReport);

        return _representante.Obtener(request, loadReport);
    }
}
