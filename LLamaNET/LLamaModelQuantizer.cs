namespace LLamaNET;

using LLamaNET.Native;

using System.Runtime.InteropServices;

/// <summary>모델 양자화에 사용되는 양자화기입니다.</summary>
public class LLamaModelQuantizer {
    private LLamaModelQuantizeParams param;

    /// <summary>새 양자화기를 초기화합니다.</summary>
    public LLamaModelQuantizer()
        => param = NativeFunctions.llama_model_quantize_default_params();

    /// <summary>양자화를 진행할 파일 타입입니다.</summary>
    public LLamaFileType FileType {
        get => param.ftype;
        set => param.ftype = value;
    }

    /// <summary>FP32 / FP16 이외, 이미 양자화된 파일에서 양자화를 진행할지 여부입니다.</summary>
    public bool AllowRequantize {
        get => param.allow_requantize != 0;
        set => param.allow_requantize = value ? (byte)1 : (byte)0;
    }

    /// <summary>출력 가중치를 양자화 할지 여부입니다.</summary>
    public bool QuantizeOutputTensor {
        get => param.quantize_output_tensor != 0;
        set => param.quantize_output_tensor = value ? (byte)1 : (byte)0;
    }

    /// <summary>양자화에 사용될 스레드의 수입니다. 0보다 같거나 적은 경우 최대 동시성을 가집니다.</summary>
    public int Threads {
        get => param.nthread;
        set => param.nthread = value;
    }

    /// <summary>양자화를 진행합니다.</summary>
    /// <param name="inputfile">양자화를 진행할 모델의 입력 파일 경로입니다.</param>
    /// <param name="outputfile">양자화가 진행된 모델의 출력 파일 경로입니다.</param>
    public void Quantize(string inputfile, string outputfile) {
        int ret = NativeFunctions.llama_model_quantize(inputfile, outputfile, in param);
        Marshal.ThrowExceptionForHR(ret);
    }
}
