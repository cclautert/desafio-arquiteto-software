using MediatR;

namespace FluxoCaixa.Domain.Notifications;

public class DomainNotification : INotification
{
    public string Key { get; private set; }
    public string Value { get; private set; }

    public DomainNotification(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public class DomainNotificationHandler : INotificationHandler<DomainNotification>
{
    private readonly List<DomainNotification> _notifications = new();

    public IReadOnlyCollection<DomainNotification> Notifications => _notifications.AsReadOnly();

    public bool HasNotifications => _notifications.Count > 0;

    public Task Handle(DomainNotification notification, CancellationToken cancellationToken)
    {
        _notifications.Add(notification);
        return Task.CompletedTask;
    }
}
