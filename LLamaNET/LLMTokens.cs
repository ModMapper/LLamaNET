namespace LLamaNET;

using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>토큰 데이터를 저장하는 토큰 저장소입니다.</summary>
public abstract class LLMTokens {
    public LLMTokens(LLamaContext context) {
        Context = context;
    }

    protected LLamaContext Context { get; }

    public abstract int Length { get; }

    protected abstract Span<LLMToken> Span { get; }

    public abstract void Add(LLMToken token);

    public virtual void AddRange(Span<LLMToken> tokens) {
        foreach(LLMToken token in tokens)
            Add(token);
    }

    public virtual void AddRange(ICollection<LLMToken> tokens) {
        foreach (LLMToken token in tokens)
            Add(token);
    }

    public virtual void AddRange(IEnumerable<LLMToken> tokens) {
        foreach(LLMToken token in tokens)
            Add(token);
    }

    /// <summary>토큰 저장소를 초기화합니다.</summary>
    public abstract void Clear();

    public void Append(string text)
        => AppendRaw(text.Replace("\r\n", "\n"), false);

    public void Append(string text, bool bos)
        => AppendRaw(text.Replace("\r\n", "\n"), bos);

    public void AppendContext(string infer, string text) {
        AppendRaw(infer, false);
        AppendBOS();
        AppendRaw(text, false);
        AppendEOS();
    }

    public void AppendRaw(string text, bool bos) {
        int size = -Context.Tokenize(text, Span<LLMToken>.Empty, bos);
        if (size <= 0x1000) {
            Span<LLMToken> buffer = stackalloc LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange(buffer);
        } else {
            LLMToken[] buffer = new LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange((ICollection<LLMToken>)buffer);
        }
    }

    /// <summary>시작 토큰을 추가합니다.</summary>
    public void AppendBOS()
        => Add(LLMToken.TokenBOS);

    /// <summary>종료 토큰을 추가합니다.</summary>
    public void AppendEOS()
        => Add(LLMToken.TokenEOS);

    /// <summary>파일로부터 토큰과 컨텍스트 정보를 불러옵니다.</summary>
    /// <param name="filepath">세션을 불러올 파일 경로입니다.</param>
    /// <returns>세션 저장의 성공 여부입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool LoadSession(string filepath) {
        if (0x4000 <= Context.ContextSize) {
            Span<LLMToken> buffer = new LLMToken[Context.ContextSize];
            int size = (int)Context.LoadSessionFile(filepath, buffer);
            if (size == -1) return false;
            AddRange(buffer[..size]);
        } else {
            Span<LLMToken> buffer = stackalloc LLMToken[Context.ContextSize];
            int size = (int)Context.LoadSessionFile(filepath, buffer);
            if (size == -1) return false;
            AddRange(buffer[..size]);
        }
        return true;
    }

    /// <summary>현재 토큰과 컨텍스트 정보를 파일에 저장합니다.</summary>
    /// <param name="filepath">세션을 저장할 파일 경로입니다.</param>
    /// <returns>세션 저장의 성공 여부입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SaveSession(string filepath)
        => Context.SaveSessionFile(filepath, Span);

    public static implicit operator ReadOnlySpan<LLMToken>(LLMTokens tokens)
        => tokens.Span;
}
