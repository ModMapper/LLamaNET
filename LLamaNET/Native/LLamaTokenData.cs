namespace LLamaNET.Native;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LLamaTokenData {
    /// <summary>token id</summary>
    public LLMToken id;
    /// <summary>log-odds of the token</summary>
    public float logit;
    /// <summary>probability of the token</summary>
    public float p;

    public LLamaTokenData(LLMToken id, float logit, float p)
        => (this.id, this.logit, this.p) = (id, logit, p);
}
