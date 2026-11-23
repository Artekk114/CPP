namespace Core.Abstractions;

public interface INotificationSink
{
    void Write(string message);
}