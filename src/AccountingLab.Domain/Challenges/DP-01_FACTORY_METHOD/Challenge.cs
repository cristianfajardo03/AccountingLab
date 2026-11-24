using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP01FactoryMethod;

public sealed record VoucherCreationRequest(Guid CompanyId, string Kind, decimal Amount, string Currency);
public sealed record VoucherDraft(Guid Id, Guid CompanyId, string Kind, decimal Amount, string Currency);
public interface IVoucherCreationChallenge{VoucherDraft Create(VoucherCreationRequest request); }
public interface IVoucherFactory{VoucherDraft Crear(VoucherCreationRequest solicitud); }

public sealed class IngresoVoucherFactory : IVoucherFactory
{
    public VoucherDraft Crear(VoucherCreationRequest solicitud)
    {
        return CrearComprobante(solicitud);
    }

    private static VoucherDraft CrearComprobante(VoucherCreationRequest solicitud)
    {
        return new VoucherDraft(
            Guid.NewGuid(),
            solicitud.CompanyId,
            "ingreso",
            solicitud.Amount,
            solicitud.Currency
        );
    }
}

public sealed class EgresoVoucherFactory : IVoucherFactory
{
    public VoucherDraft Crear(VoucherCreationRequest solicitud)
    {
        return new VoucherDraft(
            Guid.NewGuid(),
            solicitud.CompanyId,
            "egreso",
            solicitud.Amount,
            solicitud.Currency
        );
    }
}

public sealed class AjusteVoucherFactory : IVoucherFactory
{
    public VoucherDraft Crear(VoucherCreationRequest solicitud)
    {
        return new VoucherDraft(
            Guid.NewGuid(),
            solicitud.CompanyId,
            "ajuste",
            solicitud.Amount,
            solicitud.Currency
        );
    }
}

public sealed class TrasladoVoucherFactory : IVoucherFactory
{
    public VoucherDraft Crear(VoucherCreationRequest solicitud)
    {
        return new VoucherDraft(
            Guid.NewGuid(),
            solicitud.CompanyId,
            "traslado",
            solicitud.Amount,
            solicitud.Currency
        );
    }
}

public sealed class VoucherCreationChallenge : IVoucherCreationChallenge
{
    public VoucherDraft Create(VoucherCreationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IVoucherFactory fabrica = request.Kind switch
        {
            "ingreso" => new IngresoVoucherFactory(),
            "egreso" => new EgresoVoucherFactory(),
            "ajuste" => new AjusteVoucherFactory(),
            "traslado" => new TrasladoVoucherFactory(),

            _ => throw new ArgumentException(
                $"El tipo de comprobante '{request.Kind}' no está soportado.",
                nameof(request)
            )
        };

        return fabrica.Crear(request);
    }
}
