namespace LLamaNET.Inferencer;

/// <summary>토큰 추론기의 추론 상태입니다.</summary>
public enum InferenceState {
    /// <summary>토큰 추론이 진행중입니다.</summary>
    None,
    /// <summary>종료자를 만나 토큰 추론이 종료되었습니다.</summary>
    Stop,
    /// <summary>길이로 인해 토큰 생성이 중단되었습니다.</summary>
    Length,
    /// <summary>사용자에 의해 토큰 생성이 중단되었습니다.</summary>
    Abort,
}
