namespace LLamaNET;

using LLamaNET.Context;
using LLamaNET.LLamaCpp;
using System;
using System.Runtime.CompilerServices;

/// <summary>LLM 컨텍스트</summary>
public class LLMContext : IDisposable {
    /// <summary>모델과 파라미터로부터 새 컨텍스트를 생성합니다.</summary>
    /// <param name="model">컨텍스트를 생성할 모델입니다.</param>
    /// <param name="param">컨텍스트를 생성할 파라마터입니다.</param>
    public LLMContext(LLamaModel model, LLamaParams param) {
        Context = new(model, param);
        BatchSize = param.BatchSize;
    }

    /// <summary>해당 컨텍스트에 대한 리소스를 해제합니다.</summary>
    public void Dispose() {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>내부 라마 컨텍스트입니다.</summary>
    protected LLamaContext Context { get; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize { get; set; }

    /// <summary>해당 컨텍스트에 대한 토크나이저를 가져옵니다.</summary>
    /// <returns>컨텍스트에 대한 토크나이저입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLMTokenizer GetTokenizer()
        => new(Context);

    /// <summary>해당 컨텍스트의 시드 값을 설정합니다.</summary>
    /// <param name="seed">설정할 시드 값입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRandomSeed(uint seed)
        => Context.SetRandomSeed(seed);


    /// <summary>새 토큰 추론기를 생성합니다.</summary>
    /// <param name="sampler">토큰을 추론하는데 사용할 샘플러입니다.</param>
    /// <returns>생성한 토큰 추론기입니다.</returns>
    public TextInferencer CreateInferencer(LLMSampler sampler)
        => new(Context, sampler, BatchSize);


    /// <summary>새 토큰 추론기를 생성합니다.</summary>
    /// <param name="sampler">토큰을 추론하는데 사용할 샘플러입니다.</param>
    /// <param name="tokens">토큰을 추론하는데 사용할 토큰 저장소입니다.</param>
    /// <returns>생성한 토큰 추론기입니다.</returns>
    public TextInferencer CreateInferencer(LLMSampler sampler, LLMTokens tokens)
        => new(Context, sampler, tokens, BatchSize);
}
