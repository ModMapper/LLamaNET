namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>토큰에 대한 추론을 진행하는 토큰 추론기입니다.</summary>
public class TokenInferencer
{
    /// <summary>해당 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    /// <param name="batchSize">한번에 연산을 진행할 배치 크기입니다.</param>
    public TokenInferencer(LLMSession session, LLMSampler sampler, int batchSize)
    {
        Session = session;
        Sampler = sampler;
        BatchSize = batchSize;
    }

    /// <summary>추론이 진행될 세션입니다.</summary>
    public LLMSession Session { get; }

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context => (LLamaContext)Session;

    /// <summary>토큰 샘플링에 사용할 샘플러입니다.</summary>
    public LLMSampler Sampler { get; set; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize { get; }

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
        Eval(Session, index);
        index += Session.Length;
        return _Infer();

        IEnumerable<LLMToken> _Infer()
        {
            while (true)
            {
                LLMToken token = Sampler.Sample(Context, Session);
                Session.Add(token);
                if (token == LLMToken.TokenEOS) yield break;
                index++;
                yield return token;
                Eval(token, index);
            }
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
}
