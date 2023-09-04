namespace LLamaNET;

using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>세션 데이터를 저장하는 세션입니다.</summary>
public abstract class LLMSession {
    /// <summary>컨텍스트로부터 새 세션을 생성합니다.</summary>
    /// <param name="context">새 세션을 생성할 컨텍스트입니다.</param>
    public LLMSession(LLamaContext context)
        => Context = context;

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context { get; }

    /// <summary>현재 세션의 토큰 갯수를 가져옵니다.</summary>
    public abstract int Length { get; }

    /// <summary>현재 세션의 토큰에 대한 <see cref="Span{LLMToken}"/>입니다</summary>
    protected abstract ReadOnlySpan<LLMToken> Span { get; }

    /// <summary>세션에 토큰을 추가합니다.</summary>
    /// <param name="token">세션에 추가할 토큰입니다.</param>
    public abstract void Add(LLMToken token);

    /// <summary>세션에 토큰 <see cref="Span{LLMToken}"/>을 추가합니다.</summary>
    /// <param name="tokens">세션에 추가할 토큰 <see cref="Span{LLMToken}"/>입니다.</param>
    public virtual void AddRange(ReadOnlySpan<LLMToken> tokens) {
        foreach(LLMToken token in tokens)
            Add(token);
    }

    /// <summary>세션에 토큰 열거를 추가합니다.</summary>
    /// <param name="tokens">세션에 추가할 토큰 열거입니다.</param>
    public virtual void AddRange(IEnumerable<LLMToken> tokens) {
        foreach(LLMToken token in tokens)
            Add(token);
    }

    /// <summary>세션에 저장된 토큰을 초기화합니다.</summary>
    public abstract void Clear();

    /// <summary>세션에 텍스트를 변환하여 추가합니다.</summary>
    /// <param name="text">세션에 추가할 텍스트입니다.</param>
    public void Append(string text)
        => AppendRaw(text.Replace("\r\n", "\n"), false);

    /// <summary>세션에 텍스트를 변환하여 추가합니다.</summary>
    /// <param name="text">세션에 추가할 텍스트입니다.</param>
    /// <param name="bos">시작 토큰의 추가 여부입니다.</param>
    public void Append(string text, bool bos)
        => AppendRaw(text.Replace("\r\n", "\n"), bos);

    /// <summary>세션에 텍스트를 추가합니다.</summary>
    /// <param name="text">세션에 추가할 텍스트입니다.</param>
    /// <param name="bos">시작 토큰의 추가 여부입니다.</param>
    public void AppendRaw(string text, bool bos) {
        int size = -Context.Tokenize(text, Span<LLMToken>.Empty, bos);
        if (size <= 0x1000) {
            Span<LLMToken> buffer = stackalloc LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange(buffer);
        } else {
            LLMToken[] buffer = new LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange((IReadOnlyCollection<LLMToken>)buffer);
        }
    }

    /// <summary>세션에 시작 토큰을 추가합니다.</summary>
    public void AppendBOS()
        => Add(LLMToken.TokenBOS);

    /// <summary>세션에 종료 토큰을 추가합니다.</summary>
    public void AppendEOS()
        => Add(LLMToken.TokenEOS);

    /// <summary>해당 세션의 토큰에 대한 문자열을 반환합니다.</summary>
    /// <returns>해당 세션의 토큰 문자열입니다.</returns>
    public override string ToString()
        => new LLMTokenizer(Context).Detokenize(Span);

    /// <summary>파일로부터 토큰과 컨텍스트 정보를 불러옵니다.</summary>
    /// <param name="filepath">세션을 불러올 파일 경로입니다.</param>
    /// <returns>세션 저장의 성공 여부입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool LoadSession(string filepath) {
        Span<LLMToken> tokens = 0x1000 <= Context.ContextSize
            ? new LLMToken[Context.ContextSize]
            : stackalloc LLMToken[Context.ContextSize];
        int size = (int)Context.LoadSessionFile(filepath, tokens);
        if (size == -1) return false;
        AddRange(tokens[..size]);
        return true;
    }

    /// <summary>현재 토큰과 컨텍스트 정보를 파일에 저장합니다.</summary>
    /// <param name="filepath">세션을 저장할 파일 경로입니다.</param>
    /// <returns>세션 저장의 성공 여부입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SaveSession(string filepath)
        => Context.SaveSessionFile(filepath, Span);

    /// <summary>현재 세션에 대한 토큰 스판으로 변환합니다.</summary>
    /// <param name="session">토큰 스판으로 변환할 세션입니다.</param>
    public static implicit operator ReadOnlySpan<LLMToken>(LLMSession session)
        => session.Span;

    /// <summary>세션으로부터 컨텍스트를 가져옵니다.</summary>
    /// <param name="session">컨텍스트를 가져올 세션입니다.</param>
    public static explicit operator LLamaContext(LLMSession session)
        => session.Context;
}
