using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP22Visitor;

public interface IAccountingAnalysisDocument{ Guid Id { get; } }
public sealed record InvoiceAnalysisDocument(Guid Id, decimal NetAmount, decimal TaxAmount) : IAccountingAnalysisDocument;
public sealed record PaymentAnalysisDocument(Guid Id, decimal Amount) : IAccountingAnalysisDocument;
public sealed record AnalysisPostingLine(decimal Debit, decimal Credit);
public sealed record JournalAnalysisDocument(Guid Id, IReadOnlyList<AnalysisPostingLine> Lines) : IAccountingAnalysisDocument;
public sealed record DocumentAnalysisRequest(string View, IReadOnlyList<IAccountingAnalysisDocument> Documents);
public sealed record DocumentAnalysisResult(decimal TotalAmount, int DocumentCount, IReadOnlyDictionary<string, decimal> ByKind);
public interface IDocumentAnalysisChallenge{DocumentAnalysisResult Analyze(DocumentAnalysisRequest request); }

internal interface IAccountingDocumentVisitor
{
    decimal Visit(InvoiceAnalysisDocument document);
    decimal Visit(PaymentAnalysisDocument document);
    decimal Visit(JournalAnalysisDocument document);
}

internal sealed class GrossAnalysisVisitor : IAccountingDocumentVisitor
{
    public decimal Visit(InvoiceAnalysisDocument document)
    {
        return document.NetAmount + document.TaxAmount;
    }

    public decimal Visit(PaymentAnalysisDocument document)
    {
        return document.Amount;
    }

    public decimal Visit(JournalAnalysisDocument document)
    {
        return document.Lines.Sum(line => line.Debit);
    }
}

internal sealed class CountAnalysisVisitor : IAccountingDocumentVisitor
{
    public decimal Visit(InvoiceAnalysisDocument document)
    {
        return 1m;
    }

    public decimal Visit(PaymentAnalysisDocument document)
    {
        return 1m;
    }

    public decimal Visit(JournalAnalysisDocument document)
    {
        return 1m;
    }
}

public sealed class DocumentAnalysisChallenge : IDocumentAnalysisChallenge
{
    public DocumentAnalysisResult Analyze(DocumentAnalysisRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.Documents is null)
            throw new ArgumentNullException(nameof(request.Documents));

        IAccountingDocumentVisitor visitor = request.View?.Trim().ToLowerInvariant() switch
        {
            "gross" => new GrossAnalysisVisitor(),

            "count" => new CountAnalysisVisitor(),

            _ => throw new ArgumentException(
                $"Vista de análisis no soportada: {request.View}.",
                nameof(request.View))
        };

        var results = new Dictionary<string, decimal>(
            StringComparer.OrdinalIgnoreCase);

        decimal totalAmount = 0m;

        foreach (var document in request.Documents)
        {
            if (document is null)
                throw new ArgumentException(
                    "La colección contiene un documento nulo.");

            string kind;
            decimal amount;

            switch (document)
            {
                case InvoiceAnalysisDocument invoice:
                    kind = "invoice";
                    amount = visitor.Visit(invoice);
                    break;

                case PaymentAnalysisDocument payment:
                    kind = "payment";
                    amount = visitor.Visit(payment);
                    break;

                case JournalAnalysisDocument journal:
                    kind = "journal";
                    amount = visitor.Visit(journal);
                    break;

                default:
                    throw new ArgumentException(
                        $"Tipo de documento no soportado: {document.GetType().Name}.");
            }

            if (!results.ContainsKey(kind))
                results[kind] = 0m;

            results[kind] += amount;
            totalAmount += amount;
        }

        return new DocumentAnalysisResult(
            totalAmount,
            request.Documents.Count,
            results);
    }
}
