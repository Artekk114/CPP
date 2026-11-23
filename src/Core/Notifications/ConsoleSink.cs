using System;
using Core.Abstractions;

namespace Core.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Write(string message) => Console.WriteLine(message);
}