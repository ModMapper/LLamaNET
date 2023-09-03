namespace LLamaNET.Native;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LLamaGrammarElement {
    public LLamaGretype type;
    public int value; // Unicode code point or rule ID

    /// <summary>grammar element type</summary>
    public enum LLamaGretype : int {
        /// <summary>end of rule definition</summary>
        LLAMA_GRETYPE_END = 0,

        /// <summary>start of alternate definition for rule</summary>
        LLAMA_GRETYPE_ALT = 1,

        /// <summary>non-terminal element: reference to rule</summary>
        LLAMA_GRETYPE_RULE_REF = 2,

        /// <summary>terminal element: character (code point)</summary>
        LLAMA_GRETYPE_CHAR = 3,

        /// <summary>inverse char(s) ([^a], [^a-b] [^abc])</summary>
        LLAMA_GRETYPE_CHAR_NOT = 4,

        /// <summary>modifies a preceding LLAMA_GRETYPE_CHAR or LLAMA_GRETYPE_CHAR_ALT to be an inclusive range ([a-z])</summary>
        LLAMA_GRETYPE_CHAR_RNG_UPPER = 5,

        /// <summary>modifies a preceding LLAMA_GRETYPE_CHAR or LLAMA_GRETYPE_CHAR_RNG_UPPER to add an alternate char to match ([ab], [a-zA])</summary>
        LLAMA_GRETYPE_CHAR_ALT = 6,
    };
}
