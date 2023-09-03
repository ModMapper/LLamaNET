namespace LLamaNET.LLamaCpp;

public interface ILLamaTokenizer {
    public int Tokenize(string text, Span<LLMToken> tokens, bool bos);

    public string Detokenize(LLMToken token);

    public ReadOnlySpan<byte> DetokenizeUTF8(LLMToken token);
}
