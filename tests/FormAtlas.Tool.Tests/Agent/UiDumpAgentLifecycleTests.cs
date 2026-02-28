using FormAtlas.Tool.Agent;
using Xunit;

namespace FormAtlas.Tool.Tests.Agent
{
    /// <summary>
    /// Idempotent lifecycle tests for UiDumpAgent start/stop (T019).
    /// </summary>
    public class UiDumpAgentLifecycleTests
    {
        private static UiDumpOptions DefaultOptions() => new UiDumpOptions { OutputDirectory = "." };

        [Fact]
        public void NewAgent_IsNotRunning()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            Assert.False(agent.IsRunning);
        }

        [Fact]
        public void Start_SetsRunning()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            agent.Start();
            Assert.True(agent.IsRunning);
        }

        [Fact]
        public void Stop_ClearsRunning()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            agent.Start();
            agent.Stop();
            Assert.False(agent.IsRunning);
        }

        [Fact]
        public void Start_IsIdempotent()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            agent.Start();
            agent.Start(); // second call must not throw
            Assert.True(agent.IsRunning);
        }

        [Fact]
        public void Stop_IsIdempotent()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            agent.Stop(); // stop without start must not throw
            agent.Stop();
            Assert.False(agent.IsRunning);
        }

        [Fact]
        public void RequestDump_WhenRunning_FiresEvent()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            agent.Start();

            string? received = null;
            agent.DumpRequested += (_, name) => received = name;
            agent.RequestDump("TestForm");

            Assert.Equal("TestForm", received);
        }

        [Fact]
        public void RequestDump_WhenStopped_DoesNotFireEvent()
        {
            using var agent = new UiDumpAgent(DefaultOptions());
            // Not started

            string? received = null;
            agent.DumpRequested += (_, name) => received = name;
            agent.RequestDump("TestForm");

            Assert.Null(received);
        }

        [Fact]
        public void Dispose_StopsAgent()
        {
            var agent = new UiDumpAgent(DefaultOptions());
            agent.Start();
            agent.Dispose();
            Assert.False(agent.IsRunning);
        }
    }
}
