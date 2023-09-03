namespace LLamaNET.Native;

using System;
using System.Runtime.InteropServices;

public unsafe struct LLamaContextParams {
    /// <summary>RNG 생성기의 시드 값입니다. <see cref="UInt32.MaxValue"/>은 랜덤 값입니다.</summary>
    public uint seed;
    /// <summary>모델 컨텍스트 크기입니다.</summary>
    public int n_ctx;
    /// <summary>한번에 일괄 처리되는 프롬프트 크기입니다.</summary>
    public int n_batch;
    /// <summary>Grouped-Query Attention의 수 입니다.</summary>
    public int n_gqa;
    /// <summary>RMS 정규화 엡실론 갑입니다.</summary>
    public float rms_norm_eps;
    /// <summary>GPU의 VRAM에 할당할 레이어 수 입니다.</summary>
    public int n_gpu_layers;
    /// <summary>스크래치 및 작은 텐서에 사용될 메인 GPU입니다.</summary>
    public int main_gpu;
    /// <summary>각 GPU에 할당할 레이어의 수 배열입니다. (크기: llama_max_devices 함수 사용)</summary>
    public float* tensor_split;

    /// <summary>RoPE 기본 주파수입니다.</summary>
    public float rope_freq_base;
    /// <summary>RoPE 주파수 배율입니다.</summary>
    public float rope_freq_scale;

    /// <summary>진행도 콜백 함수입니다. 0에서 1 사이 값을 가져옵니다. NULL인 경우 무시됩니다.</summary>
    public delegate* unmanaged[Stdcall]<float, IntPtr, void> progress_callback;
    /// <summary>진행도 콜백에 전달할 데이터 값입니다.</summary>
    public IntPtr progress_callback_user_data;

    /// <summary>성능을 소모하여 VRAM 사용량을 줄입니다.</summary>
    public byte low_vram;
    /// <summary>mul_mat_q 커널을 사용합니다.</summary>
    public byte mul_mat_q;
    /// <summary>KV 캐시에 FP16을 사용합니다.</summary>
    public byte f16_kv;
    /// <summary>Eval이 모든 로짓을 계산합니다.</summary>
    public byte logits_all;
    /// <summary>가중치를 제외한 단어 데이터만 로드합니다.</summary>
    public byte vocab_only;
    /// <summary>가능한 경우 mmap을 사용합니다.</summary>
    public byte use_mmap;
    /// <summary>모델 데이터를 램에 상주합니다.</summary>
    public byte use_mlock;
    /// <summary>임베딩 전용 모드</summary>
    public byte embedding;

    /// <summary>진행도 콜백의 <see cref="Delegate"/> 형식입니다.</summary>
    /// <param name="progress">0에서 1사이 값을 가지는 진행도입니다.</param>
    /// <param name="ctx">전달된 데이터 값입니다.</param>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void LLamaProgressCallback(float progress, IntPtr ctx);
}
