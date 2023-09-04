namespace LLamaNET;

using LLamaNET.LLamaCpp;
using LLamaNET.Session;

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

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context { get; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize { get; set; }

    /// <summary>해당 컨텍스트의 시드 값을 설정합니다.</summary>
    /// <param name="seed">설정할 시드 값입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRandomSeed(uint seed)
        => Context.SetRandomSeed(seed);

    /// <summary>해당 컨텍스트에 대한 토크나이저를 가져옵니다.</summary>
    /// <returns>컨텍스트에 대한 토크나이저입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLMTokenizer GetTokenizer()
        => new(Context);

    /// <summary>해당 컨텍스트에 대한 추론기를 생성합니다.</summary>
    /// <param name="sampler">추론기를 샘플링 할 경우 사용할 샘플러입니다.</param>
    /// <returns>해당 컨텍스트의 추론기입니다.</returns>
    public TextInferencer CreateInferencer(LLMSampler sampler)
        => new TextInferencer(new CircularSession(Context, BatchSize), sampler, BatchSize);

    /// <summary>컨텍스트의 내부 컨텍스트를 가져옵니다.</summary>
    /// <param name="context">내부 컨텍스트를 가져올 컨텍스트입니다.</param>
    public static explicit operator LLamaContext(LLMContext context)
        => context.Context;
}
