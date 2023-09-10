namespace LLamaNET.LLamaCpp;

using LLamaNET;
using LLamaNET.Native;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

public sealed class LLamaContext : ILLamaTokenizer, IDisposable {
    private LLamaContextHandle handle;

    public LLamaContext(LLamaModel model, LLamaParams param) {
        handle = NativeFunctions.llama_new_context_with_model(model.Handle, param.Param);
        VocabSize = GetVocabSize();
        ContextSize = GetContextSize();
        EmbedSize = GetEmbedSize();
    }

    ~LLamaContext()
        => Dispose(disposing: false);

    private void Dispose(bool disposing) {
        IntPtr ptr = Interlocked.Exchange(ref Unsafe.As<LLamaContextHandle, IntPtr>(ref handle), IntPtr.Zero);
        LLamaContextHandle _handle = Unsafe.As<IntPtr, LLamaContextHandle>(ref ptr);
        if (!_handle.Empty) {
            NativeFunctions.llama_free(_handle);
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public LLamaContextHandle Handle => handle.Empty ? throw new ObjectDisposedException(nameof(LLamaContext)) : handle;

    public int VocabSize { get; }

    public int ContextSize { get; }

    public int EmbedSize { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetKVCacheTokenCount()
        => NativeFunctions.llama_get_kv_cache_token_count(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRandomSeed(uint seed)
        => NativeFunctions.llama_set_rng_seed(Handle, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public nint GetStateSize()
        => NativeFunctions.llama_get_state_size(Handle);

    public nint GetStateData(Span<byte> data) {
        if(data.Length < GetStateSize())
            throw new InternalBufferOverflowException();
        return NativeFunctions.llama_copy_state_data(Handle, ref MemoryMarshal.GetReference(data));
    }

    public nint SetStateData(ReadOnlySpan<byte> data) {
        if (data.Length < GetStateSize())
            throw new InternalBufferOverflowException();
        return NativeFunctions.llama_set_state_data(Handle, ref MemoryMarshal.GetReference(data));
    }

    public nint LoadSessionFile(string filepath, Span<LLMToken> tokens) {
        if(NativeFunctions.llama_load_session_file(Handle, filepath,
            ref MemoryMarshal.GetReference(tokens), tokens.Length, out nint count)) {
            return count;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SaveSessionFile(string filepath, ReadOnlySpan<LLMToken> tokens)
        => NativeFunctions.llama_save_session_file(Handle, filepath, MemoryMarshal.GetReference(tokens), tokens.Length);

    public void Eval(ReadOnlySpan<LLMToken> tokens, int past, int threads) {
        int ret = NativeFunctions.llama_eval(Handle, MemoryMarshal.GetReference(tokens), tokens.Length, past, threads);
        Marshal.ThrowExceptionForHR(ret);
    }

    public void EvalEmbed(ReadOnlySpan<float> embed, int past, int threads) {
        int ret = NativeFunctions.llama_eval_embd(Handle, MemoryMarshal.GetReference(embed), embed.Length, past, threads);
        Marshal.ThrowExceptionForHR(ret);
    }

    public void EvalExport(string filename) {
        int ret = NativeFunctions.llama_eval_export(Handle, filename);
        Marshal.ThrowExceptionForHR(ret);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Tokenize(string text, Span<LLMToken> tokens, bool bos)
        => NativeFunctions.llama_tokenize(Handle, text, ref MemoryMarshal.GetReference(tokens), tokens.Length, bos);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe string Detokenize(LLMToken token)
        => Marshal.PtrToStringUTF8((nint)NativeFunctions.llama_token_to_str(Handle, token)) ?? string.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlySpan<byte> DetokenizeSpan(LLMToken token)
        => MemoryMarshal.CreateReadOnlySpanFromNullTerminated(NativeFunctions.llama_token_to_str(Handle, token));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetVocabSize()
        => NativeFunctions.llama_n_vocab(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetContextSize()
        => NativeFunctions.llama_n_ctx(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEmbedSize()
        => NativeFunctions.llama_n_embd(Handle);

    public (string[] strings, float[] scores) GetVocab() {
        var scores = new float[VocabSize];
        var strings = new string[VocabSize];
        int ret = NativeFunctions.llama_get_vocab(Handle, strings, ref scores[0], VocabSize);
        Marshal.ThrowExceptionForHR(ret);
        return (strings, scores);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<float> GetLogits()
        => GetLogits(1);

    public unsafe Span<float> GetLogits(int rows) {
        float* logitptr = NativeFunctions.llama_get_logits(Handle);
        Span<float> logits = new(logitptr, rows * VocabSize);
        return logits;
    }

    public unsafe Span<float> GetEmbeddings() {
        int embed = GetEmbedSize();
        float* embedptr = NativeFunctions.llama_get_embeddings(Handle);
        Span<float> embeddings = new(embedptr, embed);
        return embeddings;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LLamaTimings GetTimings()
        => NativeFunctions.llama_get_timings(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrintTimings()
        => NativeFunctions.llama_print_timings(Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetTimings()
        => NativeFunctions.llama_reset_timings(Handle);

}
