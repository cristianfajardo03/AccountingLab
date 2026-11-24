using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP02AbstractFactory;

public sealed record AccountingProfileRequest(string Profile, string FictionalRegime);
public sealed record AccountingFamilyMember(string Role, string CompatibilityKey);
public sealed record AccountingServiceFamily(string Profile, string FictionalRegime, IReadOnlyList<AccountingFamilyMember> Members);
public interface IAccountingProfileChallenge{AccountingServiceFamily CreateFamily(AccountingProfileRequest request); }
public interface IAccountingServiceFactory{AccountingFamilyMember CrearCalculo(string claveCompatibilidad); AccountingFamilyMember CrearValidacion(string claveCompatibilidad); AccountingFamilyMember CrearReporte(string claveCompatibilidad); }

public sealed class CommercialAccountingFactory : IAccountingServiceFactory
{
    public AccountingFamilyMember CrearCalculo(string claveCompatibilidad)
        => new("calculo", claveCompatibilidad);

    public AccountingFamilyMember CrearValidacion(string claveCompatibilidad)
        => new("validacion", claveCompatibilidad);

    public AccountingFamilyMember CrearReporte(string claveCompatibilidad)
        => new("reporte", claveCompatibilidad);
}

public sealed class ServicesAccountingFactory : IAccountingServiceFactory
{
    public AccountingFamilyMember CrearCalculo(string claveCompatibilidad)
        => new("calculo", claveCompatibilidad);

    public AccountingFamilyMember CrearValidacion(string claveCompatibilidad)
        => new("validacion", claveCompatibilidad);

    public AccountingFamilyMember CrearReporte(string claveCompatibilidad)
        => new("reporte", claveCompatibilidad);
}

public sealed class AccountingProfileChallenge : IAccountingProfileChallenge
{
    public AccountingServiceFamily CreateFamily(AccountingProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IAccountingServiceFactory fabrica = request.Profile switch
        {
            "comercial" => new CommercialAccountingFactory(),
            "servicios" => new ServicesAccountingFactory(),

            _ => throw new ArgumentException(
                $"El perfil '{request.Profile}' no está soportado.",
                nameof(request)
            )
        };

        string claveCompatibilidad = request.Profile switch
        {
            "comercial" => "FAMILIA-COMERCIAL",
            "servicios" => "FAMILIA-SERVICIOS",
            _ => throw new ArgumentException("Perfil no válido.")
        };

        var miembros = new List<AccountingFamilyMember>
        {
            fabrica.CrearCalculo(claveCompatibilidad),
            fabrica.CrearValidacion(claveCompatibilidad),
            fabrica.CrearReporte(claveCompatibilidad)
        };

        return new AccountingServiceFamily(
            request.Profile,
            request.FictionalRegime,
            miembros
        );
    }
}

