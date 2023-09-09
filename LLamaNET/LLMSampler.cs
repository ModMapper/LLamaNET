namespace LLamaNET;

using LLamaNET.LLamaCpp;

/// <summary>토큰에 대한 샘플링을 하는 샘플러입니다.</summary>
public abstract class LLMSampler {
    /// <summary>주어진 최근 토큰을 사용해 토큰을 샘플링합니다.</summary>
    /// <param name="tokens">최근 사용되거나 입력된 토큰입니다.</param>
    /// <returns>샘플링되어 선별된 토큰입니다.</returns>
    public virtual LLMToken Sample(LLamaContext context, ReadOnlySpan<LLMToken> tokens) {
        LLamaCandidates candidates = new(context);

        // 로짓 프로세스 및 패널티 적용
        ApplyPenalty(ref candidates, tokens);

        // 토큰 샘플링 개시
        return Sample(ref candidates);
    }

    /// <summary>기존 토큰에 대한 패널티를 적용하여 로짓를 수정합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <param name="tokens">최근 작성한 토큰 목록입니다.</param>
    public abstract void ApplyPenalty(ref LLamaCandidates candidates, ReadOnlySpan<LLMToken> tokens);

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public abstract LLMToken Sample(ref LLamaCandidates candidates);
}