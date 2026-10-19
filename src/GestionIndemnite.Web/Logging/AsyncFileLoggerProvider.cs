using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace GestionIndemnite.Web.Logging;

public sealed class AsyncFileLoggerProvider : ILoggerProvider, IDisposable
{
    private readonly Channel<string> _channel;
    private readonly string _directory;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _writer;

    public AsyncFileLoggerProvider(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);

        _channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(10_000)
            {
                FullMode = BoundedChannelFullMode.DropWrite
            });

        _writer = Task.Run(WriteLoopAsync);
    }

    public ILogger CreateLogger(string categoryName) => new AsyncFileLogger(categoryName, _channel);

    private async Task WriteLoopAsync()
    {
        await foreach (var line in _channel.Reader.ReadAllAsync(_cts.Token))
        {
            var path = Path.Combine(_directory, $"rh-{DateTime.UtcNow:yyyyMMdd}.log");
            await File.AppendAllTextAsync(path, line + Environment.NewLine, _cts.Token);
        }
    }

    public void Dispose()
    {
        _channel.Writer.TryComplete();
        _cts.Cancel();
        _writer.Wait(TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }

    private sealed class AsyncFileLogger : ILogger
    {
        private static readonly string[] Levels = ["INF", "WRN", "ERR"];
        private readonly string _category;
        private readonly Channel<string> _channel;

        public AsyncFileLogger(string category, Channel<string> channel)
        {
            _category = category;
            _channel = channel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel is LogLevel.Information or LogLevel.Warning or LogLevel.Error;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var levelIndex = logLevel switch
            {
                LogLevel.Information => 0,
                LogLevel.Warning => 1,
                _ => 2
            };

            var message =
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{Levels[levelIndex]}] {_category}: {formatter(state, exception)}";

            if (exception is not null)
            {
                message += Environment.NewLine + exception;
            }

            _channel.Writer.TryWrite(message);
        }
    }
}
