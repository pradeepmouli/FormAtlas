using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using Json.Schema;

namespace FormAtlas.Semantic.Validation
{
    /// <summary>
    /// Validates JSON text against the semantic bundle JSON schema (Draft 2020-12).
    /// </summary>
    public sealed class SemanticSchemaValidator : ISchemaValidator
    {
        private readonly JsonSchema _schema;

        private SemanticSchemaValidator(JsonSchema schema)
        {
            _schema = schema;
        }

        public static SemanticSchemaValidator LoadFromFile(string schemaPath)
        {
            if (!File.Exists(schemaPath))
                throw new FileNotFoundException($"Schema file not found: {schemaPath}", schemaPath);

            var schemaText = File.ReadAllText(schemaPath);
            return LoadFromString(schemaText);
        }

        public static System.Threading.Tasks.Task<SemanticSchemaValidator> LoadFromFileAsync(string schemaPath)
        {
            return System.Threading.Tasks.Task.FromResult(LoadFromFile(schemaPath));
        }

        public static SemanticSchemaValidator LoadFromString(string schemaJson)
        {
            if (string.IsNullOrWhiteSpace(schemaJson))
                throw new ArgumentNullException(nameof(schemaJson));

            var schema = JsonSchema.FromText(schemaJson);
            return new SemanticSchemaValidator(schema);
        }

        public static System.Threading.Tasks.Task<SemanticSchemaValidator> LoadFromStringAsync(string schemaJson)
        {
            return System.Threading.Tasks.Task.FromResult(LoadFromString(schemaJson));
        }

        public IReadOnlyList<string> Validate(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return new[] { "JSON text is empty or null." };

            JsonNode? node;
            try
            {
                node = JsonNode.Parse(jsonText);
            }
            catch (Exception ex)
            {
                return new[] { $"JSON parse error: {ex.Message}" };
            }

            var options = new EvaluationOptions
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = false
            };

            var result = _schema.Evaluate(node, options);
            var messages = new List<string>();

            if (!result.IsValid)
            {
                CollectErrors(result, messages);
                if (messages.Count == 0)
                    messages.Add("Schema validation failed.");
            }

            return messages;
        }

        public bool IsValid(string jsonText) => Validate(jsonText).Count == 0;

        private static void CollectErrors(EvaluationResults result, List<string> messages)
        {
            if (!result.IsValid && result.Errors != null)
            {
                foreach (var kvp in result.Errors)
                    messages.Add($"{result.InstanceLocation}: {kvp.Key}: {kvp.Value}");
            }

            if (result.Details != null)
            {
                foreach (var detail in result.Details)
                    CollectErrors(detail, messages);
            }
        }
    }
}
