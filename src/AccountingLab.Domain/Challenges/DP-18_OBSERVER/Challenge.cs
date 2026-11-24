using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP18Observer;

public sealed record FinancialEvent(Guid EventId, string Kind, decimal Amount);
public interface IFinancialNotificationsChallenge
{
    void Subscribe(string subscriberId, Action<FinancialEvent> receive);
    void Unsubscribe(string subscriberId);
    IReadOnlyList<string> Publish(FinancialEvent financialEvent);
}

public sealed class FinancialNotificationsChallenge : IFinancialNotificationsChallenge
{
    private readonly Dictionary<string, Action<FinancialEvent>> _subscribers = new();

    private readonly object _lock = new();

    public void Subscribe(string subscriberId, Action<FinancialEvent> receive)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
            throw new ArgumentException(
                "El identificador del suscriptor es obligatorio.",
                nameof(subscriberId));

        if (receive is null)
            throw new ArgumentNullException(nameof(receive));

        lock (_lock)
        {
            if (_subscribers.ContainsKey(subscriberId))
            {
                throw new ArgumentException(
                    $"El suscriptor '{subscriberId}' ya está registrado.",
                    nameof(subscriberId));
            }

            _subscribers.Add(subscriberId, receive);
        }
    }

    public void Unsubscribe(string subscriberId)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
            throw new ArgumentException(
                "El identificador del suscriptor es obligatorio.",
                nameof(subscriberId));

        lock (_lock)
        {
            _subscribers.Remove(subscriberId);
        }
    }

    public IReadOnlyList<string> Publish(FinancialEvent financialEvent)
    {
        if (financialEvent is null)
            throw new ArgumentNullException(nameof(financialEvent));

        KeyValuePair<string, Action<FinancialEvent>>[] activeSubscribers;

        lock (_lock)
        {
            activeSubscribers = _subscribers
                .OrderBy(subscriber => subscriber.Key)
                .ToArray();
        }

        var recipients = new List<string>();

        foreach (var subscriber in activeSubscribers)
        {
            subscriber.Value.Invoke(financialEvent);
            recipients.Add(subscriber.Key);
        }

        return recipients.AsReadOnly();
    }
}

