namespace LLamaNET.Native;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct LLamaTokenDataArray {
    public LLamaTokenData* data;
    public nint size;
    public byte sorted;
}