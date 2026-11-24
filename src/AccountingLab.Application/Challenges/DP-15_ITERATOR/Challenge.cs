using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP15Iterator;

public sealed record LedgerRecord(string EntryCode, DateOnly Date, decimal Debit, decimal Credit);
public sealed record LedgerPage(IReadOnlyList<LedgerRecord> Records);
public interface ILedgerCursor{bool MoveNext();LedgerRecord Current { get; } }
public interface ILedgerTraversalChallenge{ILedgerCursor Open(IReadOnlyList<LedgerPage> pages); }

public sealed class LedgerCursor : ILedgerCursor
{
    private readonly IReadOnlyList<LedgerPage> _paginas;

    private int _indicePagina = 0;
    private int _indiceRegistro = -1;

    private LedgerRecord? _registroActual;
    private bool _posicionValida = false;

    public LedgerCursor(IReadOnlyList<LedgerPage> paginas)
    {
        _paginas = paginas;
    }

    public LedgerRecord Current
    {
        get
        {
            if (!_posicionValida || _registroActual is null)
            {
                throw new InvalidOperationException(
                    "El iterador no se encuentra en una posición válida."
                );
            }

            return _registroActual;
        }
    }

    public bool MoveNext()
    {
        _posicionValida = false;
        _registroActual = null;

        while (_indicePagina < _paginas.Count)
        {
            var registros = _paginas[_indicePagina].Records;

            _indiceRegistro++;

            if (_indiceRegistro < registros.Count)
            {
                _registroActual = registros[_indiceRegistro];
                _posicionValida = true;

                return true;
            }

            _indicePagina++;
            _indiceRegistro = -1;
        }

        return false;
    }
}

public sealed class LedgerTraversalChallenge : ILedgerTraversalChallenge
{
    public ILedgerCursor Open(IReadOnlyList<LedgerPage> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);

        return new LedgerCursor(pages);
    }
}

