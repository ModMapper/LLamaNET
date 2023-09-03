namespace LLamaNET;

using System.Runtime.InteropServices;

/// <summary>LLamaCpp GGML 파일의 양자화 종류입니다.</summary>
public enum LLamaFileType : int {
    LLAMA_FTYPE_ALL_F32              = 0,
    LLAMA_FTYPE_MOSTLY_F16           = 1, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q4_0          = 2, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q4_1          = 3, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q4_1_SOME_F16 = 4, // tok_embeddings.weight and output.weight are F16
    // LLAMA_FTYPE_MOSTLY_Q4_2       = 5, // support has been removed
    // LLAMA_FTYPE_MOSTLY_Q4_3       = 6, // support has been removed
    LLAMA_FTYPE_MOSTLY_Q8_0          = 7, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q5_0          = 8, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q5_1          = 9, // except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q2_K          = 10,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q3_K_S        = 11,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q3_K_M        = 12,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q3_K_L        = 13,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q4_K_S        = 14,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q4_K_M        = 15,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q5_K_S        = 16,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q5_K_M        = 17,// except 1d tensors
    LLAMA_FTYPE_MOSTLY_Q6_K          = 18,// except 1d tensors
}

/// <summary>LLamaCpp의 로그 출력 레벨입니다.</summary>
public enum LLamaLogLevel : int {
    LLAMA_LOG_LEVEL_ERROR = 2,
    LLAMA_LOG_LEVEL_WARN = 3,
    LLAMA_LOG_LEVEL_INFO = 4
}

/// <summary>performance timing information</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LLamaTimings {
    public readonly double t_start_ms;
    public readonly double t_end_ms;
    public readonly double t_load_ms;
    public readonly double t_sample_ms;
    public readonly double t_p_eval_ms;
    public readonly double t_eval_ms;

    public readonly int n_sample;
    public readonly int n_p_eval;
    public readonly int n_eval;
}