using System;
using FormAtlas.Tool.Core;

namespace FormAtlas.Tool.Agent
{
    /// <summary>
    /// Idempotent lifecycle manager for the UI dump agent.
    /// Start/Stop are safe to call multiple times.
    /// </summary>
    public sealed class UiDumpAgent : IDisposable
    {
        private readonly UiDumpOptions _options;
        private bool _running;
        private bool _disposed;

        public event EventHandler<string>? DumpRequested;

        public bool IsRunning => _running;

        public UiDumpAgent(UiDumpOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Starts the agent. Idempotent: calling Start on an already-running agent has no effect.
        /// </summary>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UiDumpAgent));
            if (_running) return;
            _running = true;
        }

        /// <summary>
        /// Stops the agent. Idempotent: calling Stop on a stopped agent has no effect.
        /// </summary>
        public void Stop()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UiDumpAgent));
            if (!_running) return;
            _running = false;
        }

        /// <summary>
        /// Raises a dump request for the given form name.
        /// Only fires if the agent is running.
        /// </summary>
        public void RequestDump(string formName)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UiDumpAgent));
            if (!_running) return;
            DumpRequested?.Invoke(this, formName);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
        }
    }
}
