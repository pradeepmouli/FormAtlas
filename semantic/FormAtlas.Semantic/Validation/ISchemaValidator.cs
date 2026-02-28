using System.Collections.Generic;

namespace FormAtlas.Semantic.Validation
{
    /// <summary>
    /// Common interface for JSON Schema validators used in the FormAtlas pipeline.
    /// Allows <see cref="IO.UiDumpBundleReader"/> to accept a validator loaded from
    /// the UI-dump schema (<c>docs/ui-dump.schema.json</c>) rather than the semantic
    /// schema, while sharing the same validation contract.
    /// </summary>
    public interface ISchemaValidator
    {
        /// <summary>
        /// Validates the given JSON text and returns a list of error messages.
        /// An empty list means the document is valid.
        /// </summary>
        IReadOnlyList<string> Validate(string jsonText);
    }
}
