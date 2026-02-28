using FormAtlas.Tool.Contracts;
using Xunit;

namespace FormAtlas.Tool.Tests.Contract
{
    /// <summary>
    /// Tests bundle compatibility behavior against 1.x schema version rules (T023).
    /// </summary>
    public class InteropCompatibilityTests
    {
        [Theory]
        [InlineData("1.0", true)]
        [InlineData("1.1", true)]
        [InlineData("1.9", true)]
        public void SameMajor_IsCompatible(string version, bool expected)
        {
            Assert.Equal(expected, SchemaVersionPolicy.IsCompatible(version));
        }

        [Theory]
        [InlineData("2.0", false)]
        [InlineData("3.1", false)]
        public void HigherMajor_IsNotCompatible_ByDefault(string version, bool expected)
        {
            Assert.Equal(expected, SchemaVersionPolicy.IsCompatible(version));
        }

        [Fact]
        public void HigherMajor_AllowHigherMajor_IsCompatible()
        {
            Assert.True(SchemaVersionPolicy.IsCompatible("2.0", allowHigherMajor: true));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid")]
        [InlineData("1")]
        public void InvalidVersionString_IsNotCompatible(string version)
        {
            Assert.False(SchemaVersionPolicy.IsCompatible(version));
        }

        [Fact]
        public void Validate_ThrowsOnIncompatibleVersion()
        {
            Assert.Throws<System.InvalidOperationException>(
                () => SchemaVersionPolicy.Validate("2.0"));
        }

        [Fact]
        public void Validate_DoesNotThrow_OnCompatibleVersion()
        {
            var exception = Record.Exception(() => SchemaVersionPolicy.Validate("1.0"));
            Assert.Null(exception);
        }
    }
}
