namespace LLamaNET.Sampler;

using LLamaNET.LLamaCpp;

using System;

/// <summary>개행 문자에 대한 패널티를 제거하는 샘플러입니다.</summary>
public class PenalizeNLSampler : LLMSampler {
    /// <summary>개행 문자에 대한 패널티를 제거하는 샘플러를 생성합니다.</summary>
    /// <param name="sampler">기존 샘플러입니다.</param>
    public PenalizeNLSampler(LLMSampler sampler)
        => BaseSampler = sampler;

    /// <summary>베이스로 사용될 샘플러입니다.</summary>
    public LLMSampler BaseSampler { get; }

    /// <summary>기존 토큰에 대한 패널티를 적용하여 로짓를 수정합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <param name="LastToken">최근 작성한 토큰 목록입니다.</param>
    public override void ApplyPenalty(ref LLamaCandidates candidates, ReadOnlySpan<LLMToken> tokens) {
        float logit = candidates[LLMToken.TokenNL].logit;
        BaseSampler.ApplyPenalty(ref candidates, tokens);
        candidates[LLMToken.TokenNL].logit = logit;
    }

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public override LLMToken Sample(LLamaCandidates candidates)
        => BaseSampler.Sample(candidates);
}
