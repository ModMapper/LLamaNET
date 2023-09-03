namespace LLamaNET.Native;

using System.Runtime.InteropServices;

/// <summary>모델 양자화 파라미터입니다.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LLamaModelQuantizeParams {
    /// <summary>양자화에 사용될 스레드의 수입니다. 0보다 같거나 적은 경우 최대 동시성을 가집니다.</summary>
    public int nthread;
    /// <summary>양자화를 진행할 파일 타입입니다.</summary>
    public LLamaFileType ftype;
    /// <summary>FP32 / FP16 이외, 이미 양자화된 파일에서 양자화를 진행할지 여부입니다.</summary>
    public byte allow_requantize;
    /// <summary>출력 가중치를 양자화 할지 여부입니다.</summary>
    public byte quantize_output_tensor;
}