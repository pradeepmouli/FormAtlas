using FormAtlas.Tool.Core;
using FormAtlas.Tool.Metadata;
using Xunit;

namespace FormAtlas.Tool.Tests.Metadata
{
    /// <summary>
    /// Reflection safety tests for missing DevExpress assemblies (T022).
    /// Validates that the adapter registry safely returns null for non-DevExpress types.
    /// </summary>
    public class DevExpressReflectionSafetyTests
    {
        [Fact]
        public void AdapterRegistry_UnknownControl_ReturnsNull()
        {
            var registry = new AdapterRegistry();
            var warnings = new PipelineWarnings();
            var control = new { Name = "PlainControl", Width = 100, Height = 100 };

            var metadata = registry.TryExtract(control, warnings);

            Assert.Null(metadata);
        }

        [Fact]
        public void AdapterRegistry_UnknownControl_DoesNotAddWarnings()
        {
            var registry = new AdapterRegistry();
            var warnings = new PipelineWarnings();
            var control = new { Name = "PlainControl", Width = 100, Height = 100 };

            registry.TryExtract(control, warnings);

            Assert.Empty(warnings.Items);
        }

        [Fact]
        public void AdapterRegistry_DoesNotThrow_WhenControlHasNoProperties()
        {
            var registry = new AdapterRegistry();
            var warnings = new PipelineWarnings();
            // Empty anonymous type with no properties
            var control = new { };

            var exception = Record.Exception(() => registry.TryExtract(control, warnings));
            Assert.Null(exception);
        }

        [Fact]
        public void PipelineWarnings_AccumulatesMultipleWarnings()
        {
            var warnings = new PipelineWarnings();
            warnings.AddWarning("CODE_1", "first warning");
            warnings.AddWarning("CODE_2", "second warning");

            Assert.Equal(2, warnings.Items.Count);
            Assert.False(warnings.HasErrors);
        }

        [Fact]
        public void PipelineWarnings_HasErrors_WhenErrorAdded()
        {
            var warnings = new PipelineWarnings();
            warnings.AddError("ERR", "critical error");

            Assert.True(warnings.HasErrors);
        }
    }
}
