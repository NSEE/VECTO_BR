using System;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Represents a prompt input during the object building process. Contains information about
    /// what to ask, what type of answer is expected, and stores the result.
    /// </summary>
    public class PromptAction
    {
        /// <summary>
        /// Gets or sets the property path in dot notation (e.g., "Components.Engine.MaxPower").
        /// Used to identify where in the object hierarchy this prompt applies.
        /// </summary>
        public required string Property { get; set; }

        /// <summary>
        /// Gets or sets the human-readable question text to display to the user.
        /// </summary>
        public required string Query { get; set; }

        /// <summary>
        /// Gets or sets the expected data type of the answer.
        /// </summary>
        public required PromptActionType Type { get; set; }

        /// <summary>
        /// Gets or sets the nesting depth of this property in the object hierarchy. Used for
        /// indentation or organizing prompts.
        /// </summary>
        public required int Depth { get; set; }

        /// <summary>
        /// Gets or sets whether this property is optional (can be null or skipped).
        /// </summary>
        public required bool Optional { get; set; }

        /// <summary>
        /// Gets or sets the default/template value from a template object. This can be used to
        /// pre-populate the answer or show a suggested value.
        /// </summary>
        public object? TemplateValue { get; set; }

        /// <summary>
        /// Gets or sets whether a template value was found and set.
        /// </summary>
        public bool TemplateValueSpecified { get; set; }

        /// <summary>
        /// Gets or sets the available options for Enum, Class, or Collection types. Key is the
        /// actual value, Value is the human-readable display text.
        /// </summary>
        public IDictionary<string, string>? Selection { get; set; }

        /// <summary>
        /// Gets or sets the valid range for numeric input (Integer or Decimal types).
        /// </summary>
        public RangeAttribute? Range { get; set; }

        /// <summary>
        /// Gets or sets the user's answer/response to this prompt. Populated by the prompt handler
        /// after user interaction.
        /// </summary>
        public object? Answer { get; set; }
    }

    /// <summary>
    /// Defines the data types that can be prompted for during object building.
    /// </summary>
    public enum PromptActionType
    {
        /// <summary>Text input</summary>
        String,

        /// <summary>Selection from a predefined set of named values</summary>
        Enum,

        /// <summary>Selection or creation of a complex object type</summary>
        Class,

        /// <summary>Whole number input</summary>
        Integer,

        /// <summary>Date and time input</summary>
        DateTime,

        /// <summary>Decimal number input</summary>
        Decimal,

        /// <summary>True/false input</summary>
        Boolean,

        /// <summary>Collection/List of items (can add multiple)</summary>
        Collection
    }
}
