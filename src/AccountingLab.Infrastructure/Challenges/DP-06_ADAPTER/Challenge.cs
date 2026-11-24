using System.Globalization;
using System.Text.Json;
using AccountingLab.Domain.Education;

namespace AccountingLab.Infrastructure.Challenges.DP06Adapter;

public sealed record BankImportRequest(string Provider, string Payload);
public sealed record ImportedBankMovement(string ExternalId, DateOnly Date, string Description, decimal Debit, decimal Credit, string Currency);
public interface IBankImportChallenge{IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request); }
public interface IBankAdapter{IReadOnlyList<ImportedBankMovement> Importar(string contenido); }

public sealed class BancoAAdapter : IBankAdapter
{
    public IReadOnlyList<ImportedBankMovement> Importar(string contenido)
    {
        using var documento = JsonDocument.Parse(contenido);

        if (documento.RootElement.ValueKind != JsonValueKind.Array)
            throw new FormatException("El contenido del BancoA debe ser un arreglo JSON.");

        var movimientos = new List<ImportedBankMovement>();

        foreach (var elemento in documento.RootElement.EnumerateArray())
        {
            var movimiento = new ImportedBankMovement(
                ExternalId: ObtenerTexto(elemento, "id"),
                Date: ObtenerFecha(elemento, "date"),
                Description: ObtenerTexto(elemento, "description"),
                Debit: ObtenerDecimal(elemento, "debit"),
                Credit: ObtenerDecimal(elemento, "credit"),
                Currency: ObtenerTexto(elemento, "currency")
            );

            movimientos.Add(movimiento);
        }

        return movimientos;
    }

    private static string ObtenerTexto(JsonElement elemento, string propiedad)
    {
        return elemento.GetProperty(propiedad).GetString()
            ?? throw new FormatException($"La propiedad '{propiedad}' no puede ser nula.");
    }

    private static decimal ObtenerDecimal(JsonElement elemento, string propiedad)
    {
        var valor = ObtenerTexto(elemento, propiedad);

        if (!decimal.TryParse(
            valor,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var resultado))
        {
            throw new FormatException($"El valor '{valor}' no es un decimal válido.");
        }

        return resultado;
    }

    private static DateOnly ObtenerFecha(JsonElement elemento, string propiedad)
    {
        var valor = ObtenerTexto(elemento, propiedad);

        if (!DateOnly.TryParseExact(
            valor,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var resultado))
        {
            throw new FormatException($"La fecha '{valor}' no tiene un formato válido.");
        }

        return resultado;
    }
}

public sealed class BancoBAdapter : IBankAdapter
{
    public IReadOnlyList<ImportedBankMovement> Importar(string contenido)
    {
        using var documento = JsonDocument.Parse(contenido);

        if (documento.RootElement.ValueKind != JsonValueKind.Array)
            throw new FormatException("El contenido del BancoB debe ser un arreglo JSON.");

        var movimientos = new List<ImportedBankMovement>();

        foreach (var elemento in documento.RootElement.EnumerateArray())
        {
            var importe = ObtenerDecimal(elemento, "signedAmount");

            var debito = importe < 0 ? decimal.Abs(importe) : 0m;
            var credito = importe > 0 ? importe : 0m;

            var movimiento = new ImportedBankMovement(
                ExternalId: ObtenerTexto(elemento, "reference"),
                Date: ObtenerFecha(elemento, "bookedOn"),
                Description: ObtenerTexto(elemento, "concept"),
                Debit: debito,
                Credit: credito,
                Currency: ObtenerTexto(elemento, "isoCurrency")
            );

            movimientos.Add(movimiento);
        }

        return movimientos;
    }

    private static string ObtenerTexto(JsonElement elemento, string propiedad)
    {
        return elemento.GetProperty(propiedad).GetString()
            ?? throw new FormatException($"La propiedad '{propiedad}' no puede ser nula.");
    }

    private static decimal ObtenerDecimal(JsonElement elemento, string propiedad)
    {
        var valor = ObtenerTexto(elemento, propiedad);

        if (!decimal.TryParse(
            valor,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var resultado))
        {
            throw new FormatException($"El valor '{valor}' no es un decimal válido.");
        }

        return resultado;
    }

    private static DateOnly ObtenerFecha(JsonElement elemento, string propiedad)
    {
        var valor = ObtenerTexto(elemento, propiedad);

        if (!DateOnly.TryParseExact(
            valor,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var resultado))
        {
            throw new FormatException($"La fecha '{valor}' no tiene un formato válido.");
        }

        return resultado;
    }
}

public sealed class BankImportChallenge : IBankImportChallenge
{
    public IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IBankAdapter adaptador = request.Provider switch
        {
            "BancoA" => new BancoAAdapter(),
            "BancoB" => new BancoBAdapter(),

            _ => throw new ArgumentException(
                $"El proveedor bancario '{request.Provider}' no está soportado.",
                nameof(request)
            )
        };

        try
        {
            return adaptador.Importar(request.Payload);
        }
        catch (JsonException ex)
        {
            throw new FormatException(
                "El contenido proporcionado no contiene un JSON válido.",
                ex
            );
        }
        catch (KeyNotFoundException ex)
        {
            throw new FormatException(
                "El contenido bancario no contiene todos los campos requeridos.",
                ex
            );
        }
        catch (InvalidOperationException ex)
        {
            throw new FormatException(
                "El contenido bancario tiene campos con tipos incorrectos.",
                ex
            );
        }
        catch (ArgumentNullException ex)
        {
            throw new FormatException(
                "El contenido bancario no puede ser nulo.",
                ex
            );
        }
    }
}

