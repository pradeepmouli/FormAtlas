using System;
using System.IO;
using FormAtlas.Semantic.Validation;
using Newtonsoft.Json.Linq;

namespace FormAtlas.Semantic.IO
{
    /// <summary>
    /// Reads and validates a UiDumpBundle from a form.json file.
    /// </summary>
    public sealed class UiDumpBundleReader
    {
        private readonly ISchemaValidator? _validator;

        public UiDumpBundleReader(ISchemaValidator? validator = null)
        {
            _validator = validator;
        }

        /// <summary>
        /// Reads and optionally validates form.json from the given path.
        /// Returns the parsed JObject for flexible access.
        /// </summary>
        public JObject ReadFromFile(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"Bundle file not found: {jsonPath}", jsonPath);

            var jsonText = File.ReadAllText(jsonPath);
            return ReadFromText(jsonText);
        }

        /// <summary>
        /// Parses and optionally validates the given JSON text.
        /// </summary>
        public JObject ReadFromText(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                throw new ArgumentException("JSON text is empty.", nameof(jsonText));

            if (_validator != null)
            {
                var errors = _validator.Validate(jsonText);
                if (errors.Count > 0)
                    throw new InvalidOperationException(
                        $"Bundle validation failed: {string.Join("; ", errors)}");
            }

            return JObject.Parse(jsonText);
        }
    }
}
