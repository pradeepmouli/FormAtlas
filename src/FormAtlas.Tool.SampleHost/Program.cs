using System;
using FormAtlas.Tool.Agent;

namespace FormAtlas.Tool.SampleHost
{
    /// <summary>
    /// Sample host that demonstrates how to integrate UiDumpAgent in a WinForms application.
    /// On Windows / net478, replace the placeholder form object with an actual Form instance.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = new UiDumpOptions
            {
                OutputDirectory = args.Length > 0 ? args[0] : ".",
                CaptureScreenshot = false,
                ExtractDevExpressMetadata = true
            };

            using var agent = new UiDumpAgent(options);
            agent.DumpRequested += (sender, formName) =>
            {
                Console.WriteLine($"[FormAtlas] Dump requested for form: {formName}");
            };

            agent.Start();
            Console.WriteLine($"[FormAtlas] Agent started. Output: {options.OutputDirectory}");
            Console.WriteLine("Press ENTER to trigger a sample dump event, or Q+ENTER to quit.");

            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                if (string.Equals(line.Trim(), "q", StringComparison.OrdinalIgnoreCase))
                    break;

                agent.RequestDump("SampleForm");
                Console.WriteLine("[FormAtlas] Dump event fired.");
            }

            agent.Stop();
            Console.WriteLine("[FormAtlas] Agent stopped.");
        }
    }
}
