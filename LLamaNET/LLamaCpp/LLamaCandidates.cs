namespace LLamaNET.LLamaCpp;
using LLamaNET.Native;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public ref struct LLamaCandidates {
    private readonly LLamaContext context;
    private readonly LLamaTokenDataArray candidates;
    private readonly LLamaTokenData[] tokendata;

    public unsafe LLamaCandidates(LLamaContext context) {
        this.context = context;
        tokendata = GC.AllocateArray<LLamaTokenData>(context.VocabSize, true);
        LLamaTokenData* ptr = (LLamaTokenData*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(tokendata));
        candidates = new() {
            data = ptr,
            size = tokendata.Length,
            sorted = 0,
        };

        Span<float> logits = context.GetLogits();
        for (int token = 0; token < context.VocabSize; token++)
            tokendata[token] = new(token, logits[token], 0);
    }

    public readonly LLamaContext Context => context;

    public readonly ref LLamaTokenData this[int index] => ref tokendata[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleRepetitionPenalty(ReadOnlySpan<LLMToken> last_tokens, float penalty)
        => NativeFunctions.llama_sample_repetition_penalty(context.Handle, candidates, MemoryMarshal.GetReference(last_tokens), last_tokens.Length, penalty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleFrequencyAndPresencePenalties(ReadOnlySpan<LLMToken> last_tokens, float alpha_frequency, float alpha_presence)
        => NativeFunctions.llama_sample_frequency_and_presence_penalties(context.Handle, candidates, MemoryMarshal.GetReference(last_tokens), last_tokens.Length, alpha_frequency, alpha_presence);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleClassifierFreeGuidance(LLamaContext guidance, float scale)
        => NativeFunctions.llama_sample_classifier_free_guidance(context.Handle, candidates, guidance.Handle, scale);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleSoftmax(LLamaContextHandle ctx, in LLamaTokenDataArray candidates)
        => NativeFunctions.llama_sample_softmax(context.Handle, candidates);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleTopK(int k, nint min_keep)
        => NativeFunctions.llama_sample_top_k(context.Handle, candidates, k, min_keep);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleTopP(float p, nint min_keep)
        => NativeFunctions.llama_sample_top_p(context.Handle, candidates, p, min_keep);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleTailFree(float z, nint min_keep)
        => NativeFunctions.llama_sample_tail_free(context.Handle, candidates, z, min_keep);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleTypical(float p, nint min_keep)
        => NativeFunctions.llama_sample_typical(context.Handle, candidates, p, min_keep);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleTemperature(float temp)
        => NativeFunctions.llama_sample_temperature(context.Handle, candidates, temp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SampleGrammar(LLamaGrammar grammar)
        => NativeFunctions.llama_sample_grammar(context.Handle, candidates, grammar.Handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LLMToken SampleTokenMirostat(float tau, float eta, int m, in float mu)
        => NativeFunctions.llama_sample_token_mirostat(context.Handle, candidates, tau, eta, m, mu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LLMToken SampleTokenMirostatV2(float tau, float eta, in float mu)
        => NativeFunctions.llama_sample_token_mirostat_v2(context.Handle, candidates, tau, eta, mu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LLMToken SampleTokenGreedy()
        => NativeFunctions.llama_sample_token_greedy(context.Handle, candidates);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LLMToken SampleToken()
        => NativeFunctions.llama_sample_token(context.Handle, candidates);

}
