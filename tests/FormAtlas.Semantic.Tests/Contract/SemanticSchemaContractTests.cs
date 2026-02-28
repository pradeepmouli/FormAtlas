using System.IO;
using System.Threading.Tasks;
using FormAtlas.Semantic.Validation;
using Xunit;

namespace FormAtlas.Semantic.Tests.Contract
{
    /// <summary>
    /// Validates semantic bundles against the semantic JSON schema (T018).
    /// </summary>
    public class SemanticSchemaContractTests
    {
        private static readonly string SchemaPath = Path.Combine("docs", "semantic.schema.json");

        [Fact]
        public async Task MinimalValidBundle_PassesSchemaValidation()
        {
            var validator = await SemanticSchemaValidator.LoadFromFileAsync(SchemaPath);
            var json = @"{
                ""semanticVersion"": ""1.0"",
                ""sourceSchemaVersion"": ""1.0"",
                ""form"": { ""name"": ""F"", ""type"": ""T"", ""width"": 100, ""height"": 100 },
                ""annotations"": [
                    {
                        ""nodeId"": ""node-0"",
                        ""roles"": [{ ""role"": ""FormRoot"", ""confidence"": 0.99 }]
                    }
                ]
            }";

            var errors = validator.Validate(json);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"semanticVersion\":\"1.0\"}")]
        [InlineData("{\"semanticVersion\":\"1.0\",\"sourceSchemaVersion\":\"1.0\",\"form\":{\"name\":\"F\",\"type\":\"T\",\"width\":100,\"height\":100}}")]
        public async Task InvalidBundle_FailsSchemaValidation(string json)
        {
            var validator = await SemanticSchemaValidator.LoadFromFileAsync(SchemaPath);

            var errors = validator.Validate(json);

            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task BundleWithRegionsAndPatterns_PassesSchemaValidation()
        {
            var validator = await SemanticSchemaValidator.LoadFromFileAsync(SchemaPath);
            var json = @"{
                ""semanticVersion"": ""1.0"",
                ""sourceSchemaVersion"": ""1.0"",
                ""form"": { ""name"": ""F"", ""type"": ""T"", ""width"": 800, ""height"": 600 },
                ""annotations"": [
                    { ""nodeId"": ""n1"", ""roles"": [{ ""role"": ""Action"", ""confidence"": 0.95, ""evidence"": [""type=Button""] }] }
                ],
                ""regions"": [
                    { ""name"": ""ActionBar"", ""bounds"": { ""x"": 0, ""y"": 560, ""w"": 800, ""h"": 40 }, ""confidence"": 0.8 }
                ],
                ""patterns"": [
                    { ""name"": ""PrimarySecondaryActions"", ""confidence"": 0.75 }
                ]
            }";

            var errors = validator.Validate(json);

            Assert.Empty(errors);
        }
    }
}
