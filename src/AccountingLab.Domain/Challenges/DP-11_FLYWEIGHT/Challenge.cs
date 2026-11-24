using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP11Flyweight;

public sealed record MovementClassificationInput(Guid Id, decimal Amount, string Code, string Label, string FictionalTaxScope);
public sealed record ClassificationMetadata(string Code, string Label, string FictionalTaxScope);
public sealed record ClassifiedMovement(Guid Id, decimal Amount, ClassificationMetadata Metadata);
public interface ISharedClassificationChallenge{IReadOnlyList<ClassifiedMovement> Prepare(IReadOnlyList<MovementClassificationInput> movements); }
public sealed class ClassificationMetadataFactory
{
    private readonly Dictionary<
        (string Code, string Label, string FictionalTaxScope),
        ClassificationMetadata
    > _metadatosCompartidos = new();

    public ClassificationMetadata Obtener(
        string codigo,
        string etiqueta,
        string alcanceFiscal
    )
    {
        var clave = (codigo, etiqueta, alcanceFiscal);

        if (_metadatosCompartidos.TryGetValue(clave, out var existente))
        {
            return existente;
        }

        var nuevosMetadatos = new ClassificationMetadata(
            codigo,
            etiqueta,
            alcanceFiscal
        );

        _metadatosCompartidos.Add(clave, nuevosMetadatos);

        return nuevosMetadatos;
    }
}

public sealed class SharedClassificationChallenge
    : ISharedClassificationChallenge
{
    public IReadOnlyList<ClassifiedMovement> Prepare(
        IReadOnlyList<MovementClassificationInput> movements
    )
    {
        ArgumentNullException.ThrowIfNull(movements);

        var fabrica = new ClassificationMetadataFactory();

        var movimientosClasificados = new List<ClassifiedMovement>();

        foreach (var movimiento in movements)
        {
            var metadatos = fabrica.Obtener(
                movimiento.Code,
                movimiento.Label,
                movimiento.FictionalTaxScope
            );

            movimientosClasificados.Add(
                new ClassifiedMovement(
                    movimiento.Id,
                    movimiento.Amount,
                    metadatos
                )
            );
        }

        return movimientosClasificados;
    }
}
