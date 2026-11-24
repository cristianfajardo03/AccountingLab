using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP09Decorator;

public sealed record DocumentExportRequest(string Body, IReadOnlyList<string> Features, IReadOnlyDictionary<string, string> Metadata);
public sealed record ExportedAccountingDocument(string Content, IReadOnlyList<string> AppliedFeatures, IReadOnlyDictionary<string, string> Metadata);
public interface IDocumentEnrichmentChallenge{ExportedAccountingDocument Export(DocumentExportRequest request); }
public interface IDocumentExporter
{
    string Content { get; }

    IReadOnlyDictionary<string, string> Metadata { get; }
}

public sealed class BasicDocumentExporter : IDocumentExporter
{
    public string Content { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public BasicDocumentExporter(string content)
    {
        Content = content;
        Metadata = new Dictionary<string, string>();
    }
}

public abstract class DocumentExporterDecorator : IDocumentExporter
{
    protected readonly IDocumentExporter Documento;

    protected DocumentExporterDecorator(IDocumentExporter documento)
    {
        Documento = documento;
    }

    public abstract string Content { get; }

    public abstract IReadOnlyDictionary<string, string> Metadata { get; }
}

public sealed class AuditDocumentDecorator : DocumentExporterDecorator
{
    public AuditDocumentDecorator(IDocumentExporter documento)
        : base(documento)
    {
    }

    public override string Content => Documento.Content + "\n[AUDIT]";

    public override IReadOnlyDictionary<string, string> Metadata
        => Documento.Metadata;
}

public sealed class MetadataDocumentDecorator : DocumentExporterDecorator
{
    private readonly IReadOnlyDictionary<string, string> _metadatos;

    public MetadataDocumentDecorator(
        IDocumentExporter documento,
        IReadOnlyDictionary<string, string> metadatos
    ) : base(documento)
    {
        _metadatos = new Dictionary<string, string>(metadatos);
    }

    public override string Content => Documento.Content;

    public override IReadOnlyDictionary<string, string> Metadata
        => _metadatos;
}

public sealed class DocumentEnrichmentChallenge
    : IDocumentEnrichmentChallenge
{
    public ExportedAccountingDocument Export(DocumentExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ArgumentNullException.ThrowIfNull(request.Features);
        ArgumentNullException.ThrowIfNull(request.Metadata);

        foreach (var funcionalidad in request.Features)
        {
            if (funcionalidad != "auditoria" &&
                funcionalidad != "metadatos")
            {
                throw new ArgumentException(
                    $"La funcionalidad '{funcionalidad}' no está soportada.",
                    nameof(request)
                );
            }
        }

        IDocumentExporter documento =
            new BasicDocumentExporter(request.Body);

        foreach (var funcionalidad in request.Features)
        {
            documento = funcionalidad switch
            {
                "auditoria" => new AuditDocumentDecorator(documento),

                "metadatos" => new MetadataDocumentDecorator(
                    documento,
                    request.Metadata
                ),

                _ => documento
            };
        }

        return new ExportedAccountingDocument(
            documento.Content,
            request.Features.ToArray(),
            new Dictionary<string, string>(documento.Metadata)
        );
    }
}


