namespace LLamaNET;

using LLamaNET.LLamaCpp;
using LLamaNET.Sampler;
using LLamaNET.Session;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>토큰에 대한 추론을 진행하는 토큰 추론기입니다.</summary>
public class LLMInferencer : IDisposable {
    /// <summary>기본 샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    public LLMInferencer(LLMSession session)
        => (Session, Sampler) = (session, new TopSampler());

    /// <summary>샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public LLMInferencer(LLMSession session, LLMSampler sampler)
        => (Session, Sampler) = (session, sampler);

    /// <summary>해당 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public LLMInferencer(LLMContext context, LLMSampler sampler)
    {
        Session = new CircularSession((LLamaContext)context, context.BatchSize);
        Sampler = sampler;
    }

    /// <summary>세션의 리소스를 해제합니다.</summary>
    public void Dispose() {
        Session.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>추론이 진행될 세션입니다.</summary>
    public LLMSession Session { get; }

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context => (LLamaContext)Session;

    /// <summary>토큰 샘플링에 사용할 샘플러입니다.</summary>
    public LLMSampler Sampler { get; set; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize => Session.BatchSize;

    /// <summary>생성에 사용할 스레드의 갯수입니다.</summary>
    public int Threads { get; set; } = LLama.MaxDevices == 1 ? Environment.ProcessorCount : 1;

    /// <summary>지정한 갯수만큼의 토큰을 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <returns>추론한 토큰의 열거입니다.</returns>
    public IEnumerable<LLMToken> Inference(int count)
        => Inference().Take(count);

    /// <summary>토큰을 추론합니다.</summary>
    /// <returns>추론한 토큰의 열거입니다.</returns>
    public IEnumerable<LLMToken> Inference()
    {
        int index = 1;
        Eval(Session.Span, index);
        index += Session.Length;

        while (true) {
            LLMToken token = Sampler.Sample(Context, Session.Span);
            if (token == LLMToken.TokenEOS) yield break;
            yield return token;
            Session.Add(token);
            index++;
            Eval(token, index);
        }
    }

    /// <summary>토큰을 비동기적으로 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <param name="cancellationToken">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>추론한 토큰의 비동기 열거입니다.</returns>
    public async IAsyncEnumerable<LLMToken> InferenceAsync(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        await foreach(LLMToken token in InferenceAsync(cancellationToken)) {
            if (Interlocked.Decrement(ref count) <= 0)
                break;
            yield return token;
        }
    }

    /// <summary>토큰을 비동기적으로 추론합니다.</summary>
    /// <param name="cancellationToken">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>추론한 토큰의 비동기 열거입니다.</returns>
    public async IAsyncEnumerable<LLMToken> InferenceAsync([EnumeratorCancellation] CancellationToken cancellationToken = default) {
        int index = 1;
        await EvalAsync(Session.Memory, index, cancellationToken);
        index += Session.Length;

        while (true) {
            LLMToken token = Sampler.Sample(Context, Session.Span);
            cancellationToken.ThrowIfCancellationRequested();
            if (token == LLMToken.TokenEOS) yield break;
            yield return token;
            Session.Add(token);
            index++;
            Eval(token, index);
        }
    }


    /// <summary>주어진 토큰에 대한 계산을 실시합니다.</summary>
    /// <param name="token">계산할 토큰입니다.</param>
    /// <param name="past">이전에 계산한 토큰의 수 입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Eval(LLMToken token, int past)
        => Context.Eval(stackalloc LLMToken[] { token }, past, 1);

    /// <summary>주어진 토큰들에 대한 계산을 배치 크기로 실시합니다.</summary>
    /// <param name="tokens">계산할 토큰들입니다.</param>
    /// <param name="past">이전에 계산한 토큰의 수 입니다.</param>
    protected void Eval(ReadOnlySpan<LLMToken> tokens, int past)
    {
        int threads = Threads;
        int index;
        for (index = 0; index < tokens.Length - BatchSize; index += BatchSize)
        {
            Context.Eval(tokens.Slice(index, BatchSize), past, threads);
            past += BatchSize;
        }
        Context.Eval(tokens[index..], past, threads);
    }

    /// <summary>주어진 토큰들에 대한 비동기적 계산을 배치 크기로 실시합니다.</summary>
    /// <param name="tokens">계산할 토큰들입니다.</param>
    /// <param name="past">이전에 계산한 토큰의 수 입니다.</param>
    /// <returns>해당 토큰 계산에 대한 Task입니다.</returns>
    protected async Task EvalAsync(ReadOnlyMemory<LLMToken> tokens, int past, CancellationToken cancellationToken = default) {
        int threads = Threads;
        int index;
        for (index = 0; index < tokens.Length - BatchSize; index += BatchSize) {
            Context.Eval(tokens.Span.Slice(index, BatchSize), past, threads);
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            past += BatchSize;
        }
        Context.Eval(tokens[index..].Span, past, threads);
        await Task.Yield();
    }
}
