namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;
using System.Text;

/// <summary>텍스트에 대한 추론을 진행하는 토큰 추론기입니다.</summary>
public class TextInferencer : TokenInferencer
{
    /// <summary>해당 컨텍스트에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="context">토큰 추론을 할 컨텍스트입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    /// <param name="tokens">토큰 추론에 사용할 토큰 저장소입니다.</param>
    /// <param name="batchSize">한번에 연산을 진행할 배치 크기입니다.</param>
    public TextInferencer(LLMSession session, LLMSampler sampler, int batchSize) : base(session, sampler, batchSize)
        => AntiPrompt = string.Empty;

    /// <summary>토큰 생성을 종료할 종료자입니다.</summary>
    public string AntiPrompt { get; set; }

    /// <summary>지정한 갯수만큼의 텍스트를 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <returns>추론한 텍스트의 열거입니다.</returns>
    public IEnumerable<string> InferenceText(int count)
        => InferenceText().Take(count);

    /// <summary>텍스트를 추론합니다.</summary>
    /// <returns>추론한 토큰의 열거입니다.</returns>
    public IEnumerable<string> InferenceText()
    {
        Decoder decoder = Encoding.UTF8.GetDecoder();
        char[] buffer = new char[0x200];
        int index = 0;

        foreach (LLMToken token in Inference())
        {
            string anti = AntiPrompt;
            var span = ((LLamaContext)Session).DetokenizeSpan(token);
            index += decoder.GetChars(span, buffer.AsSpan(index), false);

            int find = buffer.AsSpan().IndexOf(anti);
            if (find != -1)
            {   // 안티 프롬프트 발견
                yield return new(buffer.AsSpan(0, find));
                yield break;
            }

            if (anti.Length < index)
            {
                int count = index - anti.Length;
                // 서로게이트 확인
                while (0 < count && char.IsHighSurrogate(buffer[count - 1]))
                    count--;
                // 서로게이트를 제외한 크기가 0인경우 다음 추론 
                if (count == 0) continue;
                // 프롬프트 확인에 필요없는 문자열 모두 반환
                yield return new(buffer.AsSpan(0, count));
                // 남은 프롬프트를 이동한다.
                buffer.AsSpan(count, index - count).CopyTo(buffer);
                index -= count;
            }
        }
        // 남은 토큰을 전부 반환한다.
        index += decoder.GetChars(ReadOnlySpan<byte>.Empty, buffer.AsSpan(index), true);
        yield return new(buffer.AsSpan(0, index));
    }
}
