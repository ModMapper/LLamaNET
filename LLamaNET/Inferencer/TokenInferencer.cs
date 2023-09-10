namespace LLamaNET.Inferencer;
using LLamaNET.LLamaCpp;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

/// <summary>세션으로부터 토큰을 추론하는 토큰 추론기입니다.</summary>
public partial class TokenInferencer : IEnumerable<LLMToken>, IAsyncEnumerable<LLMToken> {
    private LLMToken token;
    private int past, count;

    /// <summary>새 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론기를 생성할 세션입니다.</param>
    /// <param name="sampler">토큰 추론기를 생성할 샘플러입니다.</param>
    protected TokenInferencer(LLMSession session, LLMSampler sampler)
        => (Session, Sampler) = (session, sampler);

    /// <summary>해당 세션으로부터 새 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론기를 생성할 세션입니다.</param>
    /// <param name="sampler">토큰 추론기를 생성할 샘플러입니다.</param>
    /// <param name="batchSize">토큰 계산을 실시할 배치 크기입니다.</param>
    /// <param name="threads">토큰 추론시 사용할 스레드의 수 입니다.</param>
    /// <returns>생성한 토큰 추론기입니다.</returns>
    public static TokenInferencer CreateInferencer(LLMSession session, LLMSampler sampler, int batchSize, int threads) {
        TokenInferencer inferencer = new(session, sampler) { Threads = threads };
        inferencer.Eval(session.Span, batchSize);
        return inferencer;
    }

    /// <summary>해당 세션으로부터 비 동기적으로 새 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론기를 생성할 세션입니다.</param>
    /// <param name="sampler">토큰 추론기를 생성할 샘플러입니다.</param>
    /// <param name="batchSize">토큰 계산을 실시할 배치 크기입니다.</param>
    /// <param name="threads">토큰 추론시 사용할 스레드의 수 입니다.</param>
    /// <param name="token">토큰 추론기 생성을 취소할 취소 토큰입니다.</param>
    /// <returns>생성한 토큰 추론기입니다.</returns>
    public static async Task<TokenInferencer> CreateInferencerAsync(LLMSession session, LLMSampler sampler, int batchSize, int threads, CancellationToken token = default) {
        TokenInferencer inferencer = new(session, sampler) { Threads = threads };
        await inferencer.EvalAsync(session.Memory, batchSize, token);
        return inferencer;
    }

    /// <summary>계산된 토큰의 수 입니다.</summary>
    public int Count => count;

    /// <summary>현재 추론한 토큰입니다.</summary>
    public LLMToken Token => token;

    /// <summary>추론이 진행될 세션입니다.</summary>
    public LLMSession Session { get; }

    /// <summary>토큰 샘플링에 사용할 샘플러입니다.</summary>
    public LLMSampler Sampler { get; set; }

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context => (LLamaContext)Session;

    /// <summary>최대 추론 갯수입니다.</summary>
    public int MaxTokens { get; set; }

    /// <summary>추론에 사용할 스레드의 갯수입니다.</summary>
    public int Threads { get; set; }

    /// <summary>다음 토큰을 추론합니다.</summary>
    /// <returns>토큰 추론의 성공 여부입니다.</returns>
    public InferenceState NextToken() {
        if (token == LLMToken.TokenEOS)
            return InferenceState.Stop;
        if (MaxTokens != 0 && MaxTokens <= count)
            return InferenceState.Length;
        token = Sampler.Sample(Context, Session.Span);
        Session.Add(token);
        Eval(token);
        count++;
        return InferenceState.None;
    }

    /// <summary>주어진 토큰에 대한 계산을 실시합니다.</summary>
    /// <param name="token">계산할 토큰입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Eval(LLMToken token)
        => Context.Eval(stackalloc LLMToken[] { token }, past++, Threads);

    /// <summary>주어진 토큰들에 대한 계산을 배치 크기로 실시합니다.</summary>
    /// <param name="tokens">계산할 토큰들입니다.</param>
    /// <param name="batchSize">연산을 진행할 배치 크기입니다.</param>
    protected void Eval(ReadOnlySpan<LLMToken> tokens, int batchSize) {
        int threads = Threads;
        int index;
        for (index = 0; index < tokens.Length - batchSize; index += batchSize) {
            Context.Eval(tokens.Slice(index, batchSize), past, threads);
            past += batchSize;
        }
        Context.Eval(tokens[index..], past, threads);
        past += (tokens.Length - index);
    }

    /// <summary>주어진 토큰들에 대한 계산을 배치 크기로 실시합니다.</summary>
    /// <param name="tokens">계산할 토큰들입니다.</param>
    /// <param name="batchSize">연산을 진행할 배치 크기입니다.</param>
    protected async ValueTask EvalAsync(ReadOnlyMemory<LLMToken> tokens, int batchSize, CancellationToken token = default) {
        int threads = Threads;
        int index;
        for (index = 0; index < tokens.Length - batchSize; index += batchSize) {
            token.ThrowIfCancellationRequested();
            await Task.Run(() => Context.Eval(tokens.Span.Slice(index, batchSize), past, threads), default);
            past += batchSize;
        }
        token.ThrowIfCancellationRequested();
        await Task.Run(() => Context.Eval(tokens.Span[index..], past, threads), default);
        past += (tokens.Length - index);
    }

    /// <summary>토큰 추론기을 열거자 형태로 반환합니다.</summary>
    /// <returns>열거자 형태의 토큰 추론기입니다.</returns>
    public IEnumerator<LLMToken> GetEnumerator() {
        while(NextToken() == InferenceState.None) yield return Token;
    }

    /// <summary>토큰 추론기을 열거자 형태로 반환합니다.</summary>
    /// <returns>열거자 형태의 토큰 추론기입니다.</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        while (NextToken() == InferenceState.None) yield return Token;
    }

    /// <summary>토큰 추론기을 비동기 열거자 형태로 반환합니다.</summary>
    /// <param name="cancellationToken">토큰 추론을 취소할 취소 토큰입니다.</param>
    /// <returns>비동기 열거자 형태의 토큰 추론기입니다.</returns>
    public async IAsyncEnumerator<LLMToken> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        while (await Task.Run(NextToken) == InferenceState.None) {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Token;
        }
    }
}
