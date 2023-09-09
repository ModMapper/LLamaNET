namespace LLamaNET;

using LLamaNET.LLamaCpp;
using LLamaNET.Session;

using System.Runtime.CompilerServices;

/// <summary>LLM 컨텍스트</summary>
public class LLMContext : IDisposable {
    /// <summary>컨텍스트와 파라미터로부터 새 컨텍스트를 생성합니다.</summary>
    /// <param name="model">내부적으로 사용되는 라마 컨텍스트입니다.</param>
    /// <param name="param">컨텍스트를 생성할 파라마터입니다.</param>
    protected LLMContext(LLamaContext context, LLamaParams param)
        => (Context, BatchSize) = (context, param.BatchSize);

    /// <summary>모델과 파라미터로부터 새 컨텍스트를 생성합니다.</summary>
    /// <param name="model">컨텍스트를 생성할 모델입니다.</param>
    /// <param name="param">컨텍스트를 생성할 파라마터입니다.</param>
    public static LLMContext FromModel(LLMModel model, LLamaParams param)
        => new(new((LLamaModel)model, param), param);

    /// <summary>해당 컨텍스트에 대한 리소스를 해제합니다.</summary>
    public void Dispose()
        => Context.Dispose();

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

    /// <summary>해당 컨텍스트에 대한 세션을 생성합니다.</summary>
    /// <returns>컨텍스트에 대한 세션입니다.</returns>
    public LLMSession CreateSession()
        => new CircularSession(Context, BatchSize);

    /// <summary>해당 컨텍스트에 대한 세션을 생성합니다.</summary>
    /// <param name="owned">해당 세션이 현재 컨텍스트를 소유할지 여부입니다.</param>
    /// <returns>컨텍스트에 대한 세션입니다.</returns>
    public LLMSession CreateSession(bool owned)
        => new CircularSession(Context, BatchSize, owned);

    /// <summary>해당 샘플러를 사용하는 추론기를 생성합니다.</summary>
    /// <param name="sampler">추론에 사용할 샘플러입니다.</param>
    /// <returns>새 추론기를 반환합니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TextInferencer CreateInferencer(LLMSampler sampler)
        => new(new CircularSession(Context, BatchSize, true), sampler);

    /// <summary>컨텍스트의 내부 컨텍스트를 가져옵니다.</summary>
    /// <param name="context">내부 컨텍스트를 가져올 컨텍스트입니다.</param>
    public static explicit operator LLamaContext(LLMContext context)
        => context.Context;
}
