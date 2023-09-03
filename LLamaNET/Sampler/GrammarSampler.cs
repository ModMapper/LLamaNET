namespace LLamaNET.Sampler;

using LLamaNET.LLamaCpp;

using System;

/// <summary>문법에 대한 샘플링을 추가하는 샘플러입니다.</summary>
public class GrammarSampler : LLMSampler {
    /// <summary>문법에 대한 샘플링을 추가하는 샘플러를 생성합니다.</summary>
    /// <param name="sampler">기존 샘플러입니다.</param>
    public GrammarSampler(LLMSampler sampler, LLamaGrammar grammar)
        => (BaseSampler, Grammar) = (sampler, grammar);

    /// <summary>베이스로 사용될 샘플러입니다.</summary>
    public LLMSampler BaseSampler { get; }

    /// <summary>샘플러에 적용할 문법에 대한 <see cref="LLamaGrammar"/>입니다.</summary>
    public LLamaGrammar Grammar { get; }

    /// <summary>기존 토큰에 대한 패널티를 적용하여 로짓를 수정합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <param name="LastToken">최근 작성한 토큰 목록입니다.</param>
    public override void ApplyPenalty(ref LLamaCandidates candidates, ReadOnlySpan<LLMToken> tokens)
        => BaseSampler.ApplyPenalty(ref candidates, tokens);

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public override LLMToken Sample(LLamaCandidates candidates) {
        candidates.SampleGrammar(Grammar);
        LLMToken token = BaseSampler.Sample(candidates);
        Grammar.AcceptToken(candidates.Context, token);
        return token;
    }
}
