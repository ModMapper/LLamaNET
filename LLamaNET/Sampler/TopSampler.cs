namespace LLamaNET.Sampler;

using LLamaNET.LLamaCpp;

/// <summary>기본적으로 사용되는 Top N 샘플러입니다.</summary>
public class TopSampler : PenaltySampler {
    /// <summary>상위 K개의 토큰만 확인</summary>
    public int TopK { get; set; } = 20;

    /// <summary>확률이 P 이상인 토큰만 확인</summary>
    public float TopP { get; set; } = 0.9f;

    /// <summary>TFS-Z 알고리즘</summary>
    public float TfsZ { get; set; } = 1f;

    /// <summary>Typical P 값</summary>
    public float Typical { get; set; } = 1f;

    /// <summary>샘플링 시의 토큰 온도</summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public override LLMToken Sample(LLamaCandidates candidates) {
        if (Temperature == 0) return candidates.SampleTokenGreedy();
        if (TopK <= 0) TopK = candidates.Context.VocabSize;
        candidates.SampleTopK(TopK, 1);
        candidates.SampleTailFree(TfsZ, 1);
        candidates.SampleTypical(Typical, 1);
        candidates.SampleTopP(TopP, 1);
        candidates.SampleTemperature(Temperature);
        return candidates.SampleToken();
    }
}
