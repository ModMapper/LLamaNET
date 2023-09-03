namespace LLamaNET;

using LLamaNET.Native;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>라마 모델 및 컨텍스트 생성에 사용되는 파라미터입니다.</summary>
public sealed class LLamaParams {
    public delegate void LLamaCallbackDelegate(object obj, float process);
    private LLamaContextParams.LLamaProgressCallback? processcallback;
    private readonly Lazy<float[]> tensorsplit;
    private LLamaContextParams param;

    /// <summary>새 라마 파라미터를 생성합니다.</summary>
    public unsafe LLamaParams() {
        param = NativeFunctions.llama_context_default_params();
        tensorsplit = new(() => {
            var buffer = GC.AllocateArray<float>(LLama.MaxDevices, true);
            param.tensor_split = (float*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(buffer));
            return buffer;
        });
    }

    internal LLamaContextParams Param => param;

    /// <summary>RNG 생성기의 시드 값입니다. <see cref="UInt32.MaxValue"/>은 랜덤 값입니다.</summary>
    public uint Seed {
        get => param.seed;
        set => param.seed = value;
    }

    /// <summary>모델 컨텍스트 크기입니다.</summary>
    public int ContextLength {
        get => param.n_ctx;
        set => param.n_ctx = value;
    }

    /// <summary>한번에 일괄 처리되는 프롬프트 크기입니다.</summary>
    public int BatchSize {
        get => param.n_batch;
        set => param.n_batch = value;
    }

    /// <summary>Grouped-Query Attention의 수 입니다.</summary>
    public int GroupedQueryAttention {
        get => param.n_gqa;
        set => param.n_gqa = value;
    }

    /// <summary>RMS 정규화 엡실론 갑입니다.</summary>
    public float RmsNormEpsilon {
        get => param.rms_norm_eps;
        set => param.rms_norm_eps = value;
    }

    /// <summary>GPU의 VRAM에 할당할 레이어 수 입니다.</summary>
    public int GPULayerCount {
        get => param.n_gpu_layers;
        set => param.n_gpu_layers = value;
    }

    /// <summary>스크래치 및 작은 텐서에 사용될 메인 GPU입니다.</summary>
    public int MainGPU {
        get => param.main_gpu;
        set => param.main_gpu = value;
    }

    /// <summary>각 GPU에 할당할 레이어의 수 배열입니다.</summary>
    public float[] TensorSplit
        => tensorsplit.Value;

    /// <summary>RoPE 기본 주파수입니다.</summary>
    public float RoPEFrequency {
        get => param.rope_freq_base;
        set => param.rope_freq_base = value;
    }

    /// <summary>RoPE 주파수 배율입니다.</summary>
    public float RoPEFrequencyScale {
        get => param.rope_freq_scale;
        set => param.rope_freq_scale = value;
    }

    /// <summary>진행도 콜백 함수를 설정합니다.</summary>
    /// <param name="callback">진행도 콜백 함수입니다. 0에서 1 사이 값을 가져옵니다.</param>
    /// <param name="obj">인자로 주어질 오브젝트입니다.</param>
    public unsafe void SetProcessCallback(LLamaCallbackDelegate callback, object obj) {
        param.progress_callback_user_data = IntPtr.Zero;
        processcallback = (point, data) => callback(obj, point);
        param.progress_callback =
            (delegate* unmanaged[Stdcall]<float, IntPtr, void>)
            Marshal.GetFunctionPointerForDelegate(processcallback);
    }

    /// <summary>성능을 소모하여 VRAM 사용량을 줄입니다.</summary>
    public bool LowVRAM {
        get => param.low_vram != 0;
        set => param.low_vram = value ? (byte)1 : (byte)0;
    }

    /// <summary>mul_mat_q 커널을 사용합니다.</summary>
    public bool MulMatQ {
        get => param.mul_mat_q != 0;
        set => param.mul_mat_q = value ? (byte)1 : (byte)0;
    }

    /// <summary>KV 캐시에 FP16을 사용합니다.</summary>
    public bool FP16_KV {
        get => param.f16_kv != 0;
        set => param.f16_kv = value ? (byte)1 : (byte)0;
    }

    /// <summary>Eval이 모든 로짓을 계산합니다.</summary>
    public bool EvalAllLogits {
        get => param.logits_all != 0;
        set => param.logits_all = value ? (byte)1 : (byte)0;
    }

    /// <summary>가중치를 제외한 단어 데이터만 로드합니다.</summary>
    public bool LoadVocabOnly {
        get => param.vocab_only != 0;
        set => param.vocab_only = value ? (byte)1 : (byte)0;
    }

    /// <summary>가능한 경우 mmap을 사용합니다.</summary>
    public bool UseMMap {
        get => param.use_mmap != 0;
        set => param.use_mmap = value ? (byte)1 : (byte)0;
    }

    /// <summary>모델 데이터를 램에 상주합니다.</summary>
    public bool UseMLock {
        get => param.use_mlock != 0;
        set => param.use_mlock = value ? (byte)1 : (byte)0;
    }

    /// <summary>임베딩 전용 모드</summary>
    public bool EmbeddingMode {
        get => param.embedding != 0;
        set => param.embedding = value ? (byte)1 : (byte)0;
    }
}
