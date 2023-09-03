namespace LLamaNET.Native;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LLamaModelHandle {
    private readonly IntPtr handle;

    public bool Empty => handle == IntPtr.Zero;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LLamaContextHandle {
    private readonly IntPtr handle;

    public bool Empty => handle == IntPtr.Zero;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LLamaGrammarHandle {
    private readonly IntPtr handle;

    public bool Empty => handle == IntPtr.Zero;
}