using System.IO;
using System.Threading.Tasks;
using FormAtlas.Tool.Validation;
using Xunit;

namespace FormAtlas.Tool.Tests.Contract
{
    /// <summary>
    /// Validates fixture bundles against the UI dump JSON schema (T017).
    /// </summary>
    public class UiDumpSchemaContractTests
    {
        private static readonly string SchemaPath = Path.Combine("docs", "ui-dump.schema.json");
        private static readonly string FixtureDir = Path.Combine("fixtures", "ui-dump");

        [Fact]
        public async Task ValidFixture_PassesSchemaValidation()
        {
            var validator = await SchemaValidator.LoadFromFileAsync(SchemaPath);
            var json = File.ReadAllText(Path.Combine(FixtureDir, "form.json"));

            var errors = validator.Validate(json);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"schemaVersion\":\"1.0\"}")]
        [InlineData("{\"schemaVersion\":\"1.0\",\"form\":{\"name\":\"F\",\"type\":\"T\",\"width\":100,\"height\":100}}")]
        public async Task InvalidBundle_FailsSchemaValidation(string json)
        {
            var validator = await SchemaValidator.LoadFromFileAsync(SchemaPath);

            var errors = validator.Validate(json);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task ValidBundle_MinimalFields_PassesSchemaValidation()
        {
            var validator = await SchemaValidator.LoadFromFileAsync(SchemaPath);
            var json = @"{
                ""schemaVersion"": ""1.0"",
                ""form"": { ""name"": ""F"", ""type"": ""T"", ""width"": 100, ""height"": 100 },
                ""nodes"": []
            }";

            var errors = validator.Validate(json);

            Assert.Empty(errors);
        }
    }
}
