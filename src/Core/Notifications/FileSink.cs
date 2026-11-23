using System;
using System.IO;
using Core.Abstractions;

namespace Core.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _logPath;

    public FileSink(string logPath)
    {
        _logPath = Path.GetFullPath(logPath);
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
    }

    public void Write(string message) =>
        File.AppendAllText(_logPath, message + Environment.NewLine);
}