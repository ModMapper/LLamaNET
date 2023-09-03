namespace LLamaNET.Context;
using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>토큰에 대한 추론을 진행하는 토큰 추론기입니다.</summary>
public class TokenInferencer {
    /// <summary>해당 컨텍스트에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="context">토큰 추론을 할 컨텍스트입니다.</param>
    /// <param name="batchSize">한번에 연산을 진행할 배치 크기입니다.</param>
    public TokenInferencer(LLamaContext context, int batchSize) {
        Context = context;
        Sampler = new Sampler.TopSampler();
        Tokens = new LoopTokens(context, batchSize);
        BatchSize = batchSize;
    }

    /// <summary>해당 컨텍스트에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="context">토큰 추론을 할 컨텍스트입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    /// <param name="batchSize">한번에 연산을 진행할 배치 크기입니다.</param>
    public TokenInferencer(LLamaContext context, LLMSampler sampler, int batchSize) {
        Context = context;
        Sampler = sampler;
        Tokens = new LoopTokens(context, batchSize);
        BatchSize = batchSize;
    }

    /// <summary>해당 컨텍스트에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="context">토큰 추론을 할 컨텍스트입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    /// <param name="tokens">토큰 추론에 사용할 토큰 저장소입니다.</param>
    /// <param name="batchSize">한번에 연산을 진행할 배치 크기입니다.</param>
    public TokenInferencer(LLamaContext context, LLMSampler sampler, LLMTokens tokens, int batchSize) {
        Context = context;
        Sampler = sampler;
        Tokens = tokens;
        BatchSize = batchSize;
    }

    /// <summary>라마 컨텍스트입니다.</summary>
    protected LLamaContext Context { get; }

    /// <summary>토큰 샘플링에 사용할 샘플러입니다.</summary>
    public LLMSampler Sampler { get; set; }

    /// <summary>토큰이 저장된 토큰 저장소입니다.</summary>
    public LLMTokens Tokens { get; set; }

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
    public IEnumerable<LLMToken> Inference() {
        int index = 1;
        Eval(Tokens, index);
        index += Tokens.Length;
        return _Infer();

        IEnumerable<LLMToken> _Infer() {
            while (true) {
                LLMToken token = Sampler.Sample(Context, Tokens);
                Tokens.Add(token);
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
    protected void Eval(ReadOnlySpan<LLMToken> tokens, int past) {
        int threads = Threads;
        int index;
        for (index = 0; index < (tokens.Length - BatchSize); index += BatchSize) {
            Context.Eval(tokens.Slice(index, BatchSize), past, threads);
            past += BatchSize;
        }
        Context.Eval(tokens[index..], past, threads);
    }
}
