namespace LLamaNET;

using LLamaNET.LLamaCpp;

using System;

/// <summary>LLM 모델</summary>
public class LLMModel : IDisposable {
    /// <summary>라마 모델과 파라미터로부터 새 모델을 생성합니다.</summary>
    /// <param name="model">내부적으로 사용되는 라마 모델입니다.</param>
    /// <param name="param">모델 생성에 사용된 파라미터입니다.</param>
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
    /// <returns></returns>
    public static LLMModel FromFile(string filename, LLamaParams param)
        => new(LLamaModel.FromFile(filename, param), param);

    /// <summary>내부 라마 모델입니다.</summary>
    protected LLamaModel Model { get; }

    /// <summary>컨텍스트 생성에 사용할 파라미터입니다.</summary>
    protected LLamaParams Param { get; }

    /// <summary>작업에 사용할 스레드의 갯수입니다.</summary>
    public int Threads { get; set; } = LLama.MaxDevices == 1 ? Environment.ProcessorCount : 1;

    /// <summary>모델에 LoRA를 적용합니다.</summary>
    /// <param name="lora">적용할 LoRA 모델의 파일 경로입니다.</param>
    /// <param name="basemodel">LoRA 베이스 모델의 파일 경로입니다.</param>
    public void ApplyLoRA(string lora, string basemodel)
        => Model.ApplyLoRA(lora, basemodel, Threads);

    /// <summary>새 컨텍스트를 생성합니다.</summary>
    /// <returns>생성한 컨텍스트입니다.</returns>
    public LLMContext CreateContext()
        => new(Model, Param);

    /// <summary>해당 모델에 대한 토크나이저를 가져옵니다.</summary>
    /// <returns>모델에 대한 토크나이저입니다.</returns>
    public LLMTokenizer GetTokenizer()
        => new(Model);
}
