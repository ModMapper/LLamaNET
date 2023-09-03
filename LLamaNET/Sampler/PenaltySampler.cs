namespace LLamaNET.Sampler;

using LLamaNET.LLamaCpp;

using System;

/// <summary>반복/빈도/존재 패널티를 적용하는 샘플러입니다.</summary>
public abstract class PenaltySampler : LLMSampler {
    /// <summary>반복에 대한 패널티 수치</summary>
    public float RepeatPenalty { get; set; } = 1.2f;

    /// <summary>빈도에 대한 패널티 수치</summary>
    public float FrequencyPenalty { get; set; } = 0f;

    /// <summary>존재하는 토큰에 대한 패널티 값</summary>
    public float PresencePenalty { get; set; } = 0f;

    /// <summary>마지막 반복 검색 길이</summary>
    public int LastRepeatCount { get; set; } = 0;

    /// <summary>토큰 후보를 통해 최종 토큰을 샘플링합니다.</summary>
    /// <param name="candidates">토큰 후보입니다.</param>
    /// <returns>선별된 토큰입니다.</returns>
    public override void ApplyPenalty(ref LLamaCandidates candidates, ReadOnlySpan<LLMToken> tokens) {
        int repeat = Math.Min(Math.Min(tokens.Length, LastRepeatCount), candidates.Context.ContextSize);
        candidates.SampleRepetitionPenalty(tokens[..repeat], RepeatPenalty);
        candidates.SampleFrequencyAndPresencePenalties(tokens[..repeat], FrequencyPenalty, PresencePenalty);
    }
}
