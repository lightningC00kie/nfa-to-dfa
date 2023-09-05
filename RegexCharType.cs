public enum RegexCharType
{
    Symbol,         // Represents a regular character or symbol
    Union,          // Represents the "+" operator for alternation
    Concat,  // Represents the default behavior of concatenation
    Kleene   // Represents the "*" operator for zero or more repetitions
}