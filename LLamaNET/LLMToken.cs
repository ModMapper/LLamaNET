namespace LLamaNET;

using LLamaNET.Native;

using System.Runtime.InteropServices;

/// <summary>LLM 토큰입니다.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LLMToken {
    private readonly int token;

    /// <summary>토큰 번호로부터 새 토큰을 생성합니다.</summary>
    /// <param name="token">생성할 토큰 번호입니다.</param>
    public LLMToken(int token)
        => this.token = token;

    public static implicit operator int(LLMToken token)
        => token.token;

    public static implicit operator LLMToken(int token)
        => new(token);

    /// <summary>시작 토큰입니다.</summary>
    public static LLMToken TokenBOS { get; } = NativeFunctions.llama_token_bos();
    /// <summary>종료 토큰입니다.</summary>
    public static LLMToken TokenEOS { get; } = NativeFunctions.llama_token_eos();
    /// <summary>개행 토큰입니다.</summary>
    public static LLMToken TokenNL { get; } = NativeFunctions.llama_token_nl();
}