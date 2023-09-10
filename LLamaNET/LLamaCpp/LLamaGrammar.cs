namespace LLamaNET.LLamaCpp;
using LLamaNET.Native;

using System;
using System.Runtime.CompilerServices;

public sealed class LLamaGrammar : IDisposable {
    private LLamaGrammarHandle handle;

    public unsafe LLamaGrammar()
        => handle = NativeFunctions.llama_grammar_init((LLamaGrammarElement**)IntPtr.Zero, 0, 0);

    ~LLamaGrammar()
        => Dispose(disposing: false);

    private void Dispose(bool disposing) {
        IntPtr ptr = Interlocked.Exchange(ref Unsafe.As<LLamaGrammarHandle, IntPtr>(ref handle), IntPtr.Zero);
        LLamaGrammarHandle _handle = Unsafe.As<IntPtr, LLamaGrammarHandle>(ref ptr);
        if (!_handle.Empty) {
            NativeFunctions.llama_grammar_free(_handle);
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public LLamaGrammarHandle Handle => handle.Empty ? throw new ObjectDisposedException(nameof(LLamaGrammar)) : handle;

    public void AcceptToken(LLamaContext context, LLMToken token)
        => NativeFunctions.llama_grammar_accept_token(context.Handle, Handle, token);
}
