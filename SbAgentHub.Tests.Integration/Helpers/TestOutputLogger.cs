using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SbAgentHub.Tests.Integration.Helpers
{
    /// <summary>
    /// 将 ILogger 输出重定向到 xUnit 测试输出
    /// </summary>
    public class TestOutputLogger<T> : ILogger<T>
    {
        private readonly ITestOutputHelper _output;

        public TestOutputLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                _output.WriteLine($"[{logLevel}] {typeof(T).Name}: {formatter(state, exception)}");
                if (exception != null)
                    _output.WriteLine($"  Exception: {exception}");
            }
            catch
            {
                // xUnit output may not be available during async cleanup
            }
        }
    }
}
