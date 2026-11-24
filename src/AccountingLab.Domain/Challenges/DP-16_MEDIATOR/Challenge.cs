using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP16Mediator;

public sealed record InvoiceProcessingRequest(Guid InvoiceId, decimal Amount, bool IsValid, IReadOnlyList<string> Participants);
public sealed record InvoiceProcessingResult(Guid InvoiceId, bool Published, IReadOnlyList<string> ParticipantsInvoked);
public interface IInvoiceCoordinationChallenge{InvoiceProcessingResult Process(InvoiceProcessingRequest request); }
public interface IInvoiceParticipant
{
    string Name { get; }

    bool Execute(InvoiceProcessingRequest request);
}

public sealed class InvoiceValidationParticipant : IInvoiceParticipant
{
    public string Name => "validacion";

    public bool Execute(InvoiceProcessingRequest request)
    {
        return request.IsValid;
    }
}

public sealed class InvoiceAccountingParticipant : IInvoiceParticipant
{
    public string Name => "contabilizacion";

    public bool Execute(InvoiceProcessingRequest request)
    {
        return true;
    }
}

public sealed class InvoicePublicationParticipant : IInvoiceParticipant
{
    public string Name => "publicacion";

    public bool Execute(InvoiceProcessingRequest request)
    {
        return true;
    }
}

public sealed class InvoiceMediator
{
    private readonly Dictionary<string, IInvoiceParticipant> _participantes;

    public InvoiceMediator(IEnumerable<IInvoiceParticipant> participantes)
    {
        ArgumentNullException.ThrowIfNull(participantes);

        _participantes = new Dictionary<string, IInvoiceParticipant>(
            StringComparer.Ordinal
        );

        foreach (var participante in participantes)
        {
            if (participante is null)
            {
                throw new ArgumentException(
                    "Los participantes no pueden ser nulos."
                );
            }

            if (!_participantes.TryAdd(
                participante.Name,
                participante))
            {
                throw new ArgumentException(
                    $"El participante '{participante.Name}' ya está registrado."
                );
            }
        }
    }

    public InvoiceProcessingResult Procesar(
        InvoiceProcessingRequest solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        var participantesInvocados = new List<string>();

        var participantesProcesados = new HashSet<string>(
            StringComparer.Ordinal
        );

        foreach (var nombre in solicitud.Participants)
        {
            if (!participantesProcesados.Add(nombre))
            {
                throw new ArgumentException(
                    $"El participante '{nombre}' está registrado más de una vez."
                );
            }

            if (!_participantes.TryGetValue(
                nombre,
                out var participante))
            {
                throw new ArgumentException(
                    $"No existe el participante: {nombre}"
                );
            }

            participantesInvocados.Add(nombre);

            bool resultado = participante.Execute(solicitud);

            if (!resultado)
            {
                return new InvoiceProcessingResult(
                    solicitud.InvoiceId,
                    false,
                    participantesInvocados
                );
            }
        }

        return new InvoiceProcessingResult(
            solicitud.InvoiceId,
            true,
            participantesInvocados
        );
    }
}

public sealed class InvoiceCoordinationChallenge
    : IInvoiceCoordinationChallenge
{
    public InvoiceProcessingResult Process(
        InvoiceProcessingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participantes = new List<IInvoiceParticipant>
        {
            new InvoiceValidationParticipant(),
            new InvoiceAccountingParticipant(),
            new InvoicePublicationParticipant()
        };

        var mediator = new InvoiceMediator(participantes);

        return mediator.Procesar(request);
    }
}