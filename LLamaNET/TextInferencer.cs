namespace LLamaNET;

using LLamaNET.Session;

using System.Runtime.CompilerServices;
using System.Text;

/// <summary>기본 텍스트 추론기입니다.</summary>
public class TextInferencer : LLMInferencer {
    /// <summary>기본 샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    public TextInferencer(LLMSession session) : base(session) { }

    /// <summary>샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public TextInferencer(LLMSession session, LLMSampler sampler) : base(session, sampler) { }

    /// <summary>해당 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public TextInferencer(LLMContext context, LLMSampler sampler) : base(context, sampler) { }

    /// <summary>토큰 생성을 종료할 종료자입니다.</summary>
    public string AntiPrompt { get; set; } = string.Empty;

    /// <summary>지정한 갯수만큼의 텍스트를 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <returns>추론한 텍스트의 열거입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<string> InferenceText(int count)
        => InferText(Inference(count));

    /// <summary>텍스트를 추론합니다.</summary>
    /// <returns>추론한 토큰의 열거입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<string> InferenceText()
        => InferText(Inference());

    private IEnumerable<string> InferText(IEnumerable<LLMToken> tokens) {
        string anti = AntiPrompt;
        char[] buffer = new char[anti.Length + 1];
        Decoder coder = Encoding.UTF8.GetDecoder();
        int index = 0;
        string str;
        foreach (var token in tokens) {
            str = GetString(Context.DetokenizeSpan(token), false);
            if(str.Length != 0)
                yield return str;
            if (buffer.AsSpan(0, index).EndsWith(anti))
                yield break;
        }
        str = GetString(ReadOnlySpan<byte>.Empty, true);
        if (str.Length != 0)
            yield return str;
        if (!buffer.AsSpan(0, index).EndsWith(anti))
            yield return new(buffer.AsSpan(0, index));

        string GetString(ReadOnlySpan<byte> span, bool flush) {
            int count = coder.GetCharCount(span, flush);
            if (count == 0) return string.Empty;
            if (anti.Length < index + count) {
                Span<char> buf = stackalloc char[index + count];
                buffer.AsSpan(0, index).CopyTo(buf);
                coder.GetChars(span, buf[index..], flush);
                count = buf.Length - anti.Length;
                if (!char.IsHighSurrogate(buf[count]))
                    count++;
                buf[count..].CopyTo(buffer);
                index = buf.Length - count;
                return new(buf[..count]);
            } else {
                coder.GetChars(span, buffer.AsSpan(index), flush);
                index += count;
                return string.Empty;
            }
        }
    }

    /// <summary>지정한 갯수만큼의 텍스트를 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <returns>추론한 텍스트 결과입니다.</returns>
    public string InferenceTextAll(int count) {
        StringBuilder sb = new();
        foreach(var text in InferenceText(count))
            sb.Append(text);
        return sb.ToString();
    }

    /// <summary>텍스트를 비동기적으로 추론합니다.</summary>
    /// <returns>추론한 토큰의 비동기 열거입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IAsyncEnumerable<string> InferenceTextAsync(CancellationToken cancellationToken = default)
        => InferTextAsync(InferenceAsync(cancellationToken));

    /// <summary>텍스트를 비동기적으로 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 갯수입니다.</param>
    /// <returns>추론한 토큰의 비동기 열거입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IAsyncEnumerable<string> InferenceTextAsync(int count, CancellationToken cancellationToken = default)
        => InferTextAsync(InferenceAsync(count, cancellationToken));

    private async IAsyncEnumerable<string> InferTextAsync(IAsyncEnumerable<LLMToken> tokens) {
        string anti = AntiPrompt;
        char[] buffer = new char[anti.Length + 1];
        Decoder coder = Encoding.UTF8.GetDecoder();
        int index = 0;
        string str;
        await foreach (var token in tokens) {
            str = GetString(Context.DetokenizeSpan(token), false);
            if (str.Length != 0)
                yield return str;
            if (buffer.AsSpan(0, index).EndsWith(anti))
                yield break;
        }
        str = GetString(ReadOnlySpan<byte>.Empty, true);
        if (str.Length != 0)
            yield return str;
        if (!buffer.AsSpan(0, index).EndsWith(anti))
            yield return new(buffer.AsSpan(0, index));

        string GetString(ReadOnlySpan<byte> span, bool flush) {
            int count = coder.GetCharCount(span, flush);
            if (count == 0) return string.Empty;
            if (anti.Length < index + count) {
                Span<char> buf = stackalloc char[index + count];
                buffer.AsSpan(0, index).CopyTo(buf);
                coder.GetChars(span, buf[index..], flush);
                count = buf.Length - buffer.Length;
                if (!char.IsHighSurrogate(buf[count]))
                    count++;
                buf[count..].CopyTo(buffer);
                index = buf.Length - count;
                return new(buf[..count]);
            } else {
                coder.GetChars(span, buffer.AsSpan(index), flush);
                index += count;
                return string.Empty;
            }
        }
    }
}
