namespace LLamaNET;

using LLamaNET.LLamaCpp;

using System.Text;

/// <summary>토큰화나 문자열 변환을 담당하는 토크나이저입니다.</summary>
public readonly struct LLMTokenizer {
    private readonly ILLamaTokenizer tokenizer;

    /// <summary>해당 모델 또는 컨텍스트에서 토크나이저를 생성합니다.</summary>
    /// <param name="tokenizer">토큰화를 지원하는 객체입니다.</param>
    public LLMTokenizer(ILLamaTokenizer tokenizer)
        => this.tokenizer = tokenizer;

    /// <summary>해당 문자열에 대한 토큰을 가져옵니다.</summary>
    /// <param name="text">토큰을 가져올 문자열입니다.</param>
    /// <returns>토큰에 대한 배열입니다.</returns>
    public readonly LLMToken[] Tokenize(string text)
        => Tokenize(text, false);

    /// <summary>해당 문자열에 대한 토큰을 가져옵니다.</summary>
    /// <param name="text">토큰을 가져올 문자열입니다.</param>
    /// <param name="bos">문단 시작 토큰의 추가 여부입니다.</param>
    /// <returns>토큰에 대한 배열입니다.</returns>
    public readonly LLMToken[] Tokenize(string text, bool bos) {
        LLMToken[] tokens = new LLMToken[Calculate(text, bos)];
        tokenizer.Tokenize(text, tokens, bos);
        return tokens;
    }

    /// <summary>해당 문자열에 대한 토큰을 가져옵니다.</summary>
    /// <param name="text">토큰을 가져올 문자열입니다.</param>
    /// <param name="buffer">토큰을 복사할 토큰의 수 입니다.</param>
    /// <param name="bos">문단 시작 토큰의 추가 여부입니다.</param>
    /// <returns>텍스트에 대한 토큰 갯수입니다.</returns>
    public readonly int Tokenize(string text, Span<LLMToken> buffer, bool bos)
        => Math.Abs(tokenizer.Tokenize(text, buffer, bos));

    /// <summary>해당 텍스트의 토큰 수를 계산합니다.</summary>
    /// <param name="text">토큰 수를 계산할 텍스트입니다.</param>
    /// <returns>텍스트에 대한 토큰 갯수입니다.</returns>
    public readonly int Calculate(string text)
        => Calculate(text, false);

    /// <summary>해당 텍스트의 토큰 수를 계산합니다.</summary>
    /// <param name="text">토큰 수를 계산할 텍스트입니다.</param>
    /// <param name="bos">문단 시작 토큰의 추가 여부입니다.</param>
    /// <returns>텍스트에 대한 토큰 갯수입니다.</returns>
    public readonly int Calculate(string text, bool bos)
        => -tokenizer.Tokenize(text, Span<LLMToken>.Empty, bos);

    /// <summary>해당 토큰에 대한 문자열을 가져옵니다.</summary>
    /// <param name="token">문자열을 가져올 토큰입니다.</param>
    /// <returns>해당 토큰에 대한 문자열입니다.</returns>
    public readonly string Detokenize(LLMToken token)
        => tokenizer.Detokenize(token);

    /// <summary>해당 토큰들에 대한 문자열을 가져옵니다.</summary>
    /// <param name="tokens">문자열을 가져올 토큰들입니다.</param>
    /// <returns>해당 토큰에 대한 문자열입니다.</returns>
    public readonly string Detokenize(ReadOnlySpan<LLMToken> tokens) {
        using MemoryStream stream = new();
        foreach (var token in tokens)
            stream.Write(tokenizer.DetokenizeSpan(token));
        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }

    /// <summary>해당 토큰 열거에 대한 문자열을 가져옵니다.</summary>
    /// <param name="tokens">문자열을 가져올 토큰 열거입니다.</param>
    /// <returns>해당 토큰에 대한 문자열입니다.</returns>
    public readonly string Detokenize(IEnumerable<LLMToken> tokens) {
        using MemoryStream stream = new();
        foreach (var token in tokens)
            stream.Write(tokenizer.DetokenizeSpan(token));
        return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
    }
}
