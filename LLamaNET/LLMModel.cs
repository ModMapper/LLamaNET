namespace LLamaNET;

using LLamaNET.LLamaCpp;
using LLamaNET.Session;

using System;
using System.Runtime.CompilerServices;

/// <summary>LLM 모델</summary>
public class LLMModel : IDisposable {
    /// <summary>라마 모델과 파라미터로부터 새 모델을 생성합니다.</summary>
    /// <param name="model">내부적으로 사용되는 라마 모델입니다.</param>
    /// <param name="param">모델 생성에 사용되는 파라미터입니다.</param>
    protected LLMModel(LLamaModel model, LLamaParams param)
        => (Model, Param) = (model, param);

    /// <summary>해당 모델에 대한 리소스를 해제합니다.</summary>
    public void Dispose() {
        Model.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>파일로부터 모델을 불러옵니다.</summary>
    /// <param name="filename">모델 파일의 경로입니다.</param>
    /// <param name="param">모델을 불러올 때 사용할 파라미터입니다.</param>
    /// <returns>파일로부터 가져온 모델입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LLMModel FromFile(string filename, LLamaParams param)
        => new(LLamaModel.FromFile(filename, param), param);

    /// <summary>내부 라마 모델입니다.</summary>
    protected LLamaModel Model { get; }

    /// <summary>컨텍스트 생성에 사용할 파라미터입니다.</summary>
    protected LLamaParams Param { get; }

    /// <summary>모델에 LoRA를 적용합니다.</summary>
    /// <param name="lora">적용할 LoRA 모델의 파일 경로입니다.</param>
    /// <param name="basemodel">LoRA 베이스 모델의 파일 경로입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyLoRA(string lora, string basemodel)
        => Model.ApplyLoRA(lora, basemodel, LLama.MaxDevices == 1 ? Environment.ProcessorCount : 1);

    /// <summary>모델에 LoRA를 적용합니다.</summary>
    /// <param name="lora">적용할 LoRA 모델의 파일 경로입니다.</param>
    /// <param name="basemodel">LoRA 베이스 모델의 파일 경로입니다.</param>
    /// <param name="threads">동시에 작업할 스레드의 수 입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyLoRA(string lora, string basemodel, int threads)
        => Model.ApplyLoRA(lora, basemodel, threads);

    /// <summary>해당 모델에 대한 토크나이저를 가져옵니다.</summary>
    /// <returns>모델에 대한 토크나이저입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLMTokenizer GetTokenizer()
        => new(Model);

    /// <summary>현재 모델에 대한 새 컨텍스트를 생성합니다.</summary>
    /// <returns>생성된 모델 컨텍스트입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLMContext CreateContext()
        => LLMContext.FromModel(this, Param);

    /// <summary>해당 샘플러를 사용하는 추론기를 생성합니다.</summary>
    /// <param name="sampler">추론에 사용할 샘플러입니다.</param>
    /// <returns>새 추론기를 반환합니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLMInferencer CreateInferencer(LLMSampler sampler)
        => new(new CircularSession(new(Model, Param), Param.BatchSize, true), sampler);


    /// <summary>모델의 내부 모델을 가져옵니다.</summary>
    /// <param name="context">내부 모델을 가져올 모델입니다.</param>
    public static explicit operator LLamaModel(LLMModel model)
        => model.Model;
}
