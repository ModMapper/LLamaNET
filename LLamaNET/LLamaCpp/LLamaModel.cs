namespace LLamaNET.LLamaCpp;

using LLamaNET;
using LLamaNET.Native;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed class LLamaModel : ILLamaTokenizer, IDisposable {
    private LLamaModelHandle handle;

    private LLamaModel(LLamaModelHandle handle) {
        this.handle = handle;
        VocabSize = GetVocabSize();
        ContextSize = GetContextSize();
        EmbedSize = GetEmbedSize();
    }

    ~LLamaModel()
        => Dispose(disposing: false);

    public static LLamaModel FromFile(string filename, LLamaParams param)
        => new(NativeFunctions.llama_load_model_from_file(filename, param.Param));

    private void Dispose(bool disposing) {
        IntPtr ptr = Interlocked.Exchange(ref Unsafe.As<LLamaModelHandle, IntPtr>(ref handle), IntPtr.Zero);
        LLamaModelHandle _handle = Unsafe.As<IntPtr, LLamaModelHandle>(ref ptr);
        if (!_handle.Empty) {
            NativeFunctions.llama_free_model(_handle);
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public LLamaModelHandle Handle => handle.Empty ? throw new ObjectDisposedException(nameof(LLamaModel)) : handle;

    public int VocabSize { get; }

    public int ContextSize { get; }

    public int EmbedSize { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyLoRA(string lora, string basemodel, int threads) {
        int ret = NativeFunctions.llama_model_apply_lora_from_file(Handle, lora, basemodel, threads);
        Marshal.ThrowExceptionForHR(ret);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Tokenize(string text, Span<LLMToken> tokens, bool bos)
        => NativeFunctions.llama_tokenize_with_model(Handle, text, ref MemoryMarshal.GetReference(tokens), tokens.Length, bos);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string Detokenize(LLMToken token)
        => Marshal.PtrToStringUTF8((nint)NativeFunctions.llama_token_to_str_with_model(Handle, token)) ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlySpan<byte> DetokenizeSpan(LLMToken token)
        => MemoryMarshal.CreateReadOnlySpanFromNullTerminated(NativeFunctions.llama_token_to_str_with_model(Handle, token));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetVocabSize()
        => NativeFunctions.llama_n_vocab_from_model(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetContextSize()
        => NativeFunctions.llama_n_ctx_from_model(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEmbedSize()
        => NativeFunctions.llama_n_embd_from_model(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ModelType(Span<byte> buffer) {
        int ret = NativeFunctions.llama_model_type(Handle, ref MemoryMarshal.GetReference(buffer), buffer.Length);
        Marshal.ThrowExceptionForHR(ret);
    }

    public (string[] strings, float[] scores) GetVocab() {
        var scores = new float[VocabSize];
        var strings = new string[VocabSize];
        int ret = NativeFunctions.llama_get_vocab_from_model(Handle, strings, ref scores[0], VocabSize);
        Marshal.ThrowExceptionForHR(ret);
        return (strings, scores);
    }
}
