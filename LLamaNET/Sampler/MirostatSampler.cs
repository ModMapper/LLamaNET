namespace LLamaNET.Sampler;

using LLamaNET.LLamaCpp;

/// <summary>Mirostat 알고리즘을 사용하는 샘플러입니다.</summary>
public class MirostatSampler : PenaltySampler {
    private float mu;

    /// <summary>Mirostat 알고리즘을 사용하는 샘플러를 생성합니다.</summary>
    /// <param name="tau">Mirostat Tau</param>
    /// <param name="eta">Mirostat Eta</param>
    public MirostatSampler(float tau = 5f, float eta = 0.1f) {
        (Tau, Eta) = (tau, eta);
        mu = tau * 2;
    }

    /// <summary>Mirostat Tau</summary>
    public float Tau { get; set; }

    /// <summary>Mirostat Eta</summary>
    public float Eta { get; set; }

    /// <summary>Mirostat Mu</summary>
    public float Mu { get => mu; set => mu = value; }

    /// <summary>샘플링 시의 토큰 온도</summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public override LLMToken Sample(ref LLamaCandidates candidates) {
        if (Temperature == 0) return candidates.SampleTokenGreedy();
        candidates.SampleTemperature(Temperature);
        return candidates.SampleTokenMirostat(Tau, Eta, 100, mu);
    }
}
