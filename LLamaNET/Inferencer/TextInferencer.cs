namespace LLamaNET.Inferencer;

using LLamaNET.LLamaCpp;

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

/// <summary>세션으로부터 텍스트를 추론하는 텍스트 추론기입니다.</summary>
public partial class TextInferencer : IEnumerable<string>, IAsyncEnumerable<string> {
    private readonly Decoder decoder;
    private readonly char[] buffer;

    private string text;
    private int index;
    private int state;

    /// <summary>토큰 추론기로부터 문자열 추론기를 생성합니다.</summary>
    /// <param name="inferencer">토큰 추론기입니다.</param>
    /// <param name="antiPrompt">토큰 생성을 종료할 문자열입니다.</param>
    public TextInferencer(TokenInferencer inferencer, string antiPrompt) {
        buffer = new char[antiPrompt.Length + 1];
        decoder = Encoding.UTF8.GetDecoder();
        Inferencer = inferencer;
        AntiPrompt = antiPrompt;
        text = string.Empty;
    }

    /// <summary>현재 추론한 텍스트입니다.</summary>
    public string Text => text;

    /// <summary>추론기의 현재 상태입니다.</summary>
    public TokenInferencer Inferencer { get; }

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context => (LLamaContext)Inferencer.Session;

    /// <summary>토큰 생성을 종료할 종료 프롬프트입니다.</summary>
    public string AntiPrompt { get; }

    /// <summary>다음 텍스트를 추론합니다.</summary>
    /// <returns>텍스트 추론의 성공 여부입니다.</returns>
    public bool NextText() {
        switch(state) {
        case 0: goto loopstart;
        case 1: goto return1;
        case 2: goto return2;
        default: return false;
        }
    loopstart:;
        if (Inferencer.NextToken() != InferenceState.None) goto loopend;
        text = GetString(Context.DetokenizeSpan(Inferencer.Token), false);
        if (text.Length != 0) {
            state = 1;
            return true;
        }
    return1:;
        if (0 < AntiPrompt.Length && buffer.AsSpan(0, index).EndsWith(AntiPrompt)) {
            state = -1;
            text = string.Empty;
            return false;
        }
        goto loopstart;
    loopend:;
        text = GetString(ReadOnlySpan<byte>.Empty, true);
        if (text.Length != 0) {
            state = 2;
            return true;
        }
    return2:;
        state = -1;
        if (0 < AntiPrompt.Length && buffer.AsSpan(0, index).EndsWith(AntiPrompt)) {
            text = string.Empty;
            return false;
        }
        text = new(buffer.AsSpan(0, index));
        return true;
    }

    private string GetString(ReadOnlySpan<byte> span, bool flush) {
        Span<char> buf = stackalloc char[index + span.Length + 1];
        buffer.AsSpan(0, index).CopyTo(buf);
        index += decoder.GetChars(span, buf[index..], flush);
        if(AntiPrompt.Length < index) {
            int last = index - buffer.Length;
            if (!char.IsHighSurrogate(buf[last]))
                last++;
            buf[last..index].CopyTo(buffer);
            index -= last; 
            return new(buf[..last]);
        } else {
            buf[..index].CopyTo(buffer);
            return string.Empty;
        }

        /*


        int count = 0x100; // decoder.GetCharCount(span, flush);
        if (count == 0) return string.Empty;
        if (AntiPrompt.Length < index + count) {
            Span<char> buf = stackalloc char[index + count];
            buffer.AsSpan(0, index).CopyTo(buf);
            decoder.GetChars(span, buf[index..], flush);
            count = buf.Length - buffer.Length;
            if (!char.IsHighSurrogate(buf[count]))
                count++;
            buf[count..].CopyTo(buffer);
            index = buf.Length - count;
            return new(buf[..count]);
        } else {
            decoder.GetChars(span, buffer.AsSpan(index), flush);
            index += count;
            return string.Empty;
        }
        */
    }

    /// <summary>텍스트 추론기을 열거자 형태로 반환합니다.</summary>
    /// <returns>열거자 형태의 텍스트 추론기입니다.</returns>
    public IEnumerator<string> GetEnumerator() {
        while (NextText()) yield return Text;
    }

    /// <summary>텍스트 추론기을 열거자 형태로 반환합니다.</summary>
    /// <returns>열거자 형태의 텍스트 추론기입니다.</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        while (NextText()) yield return Text;
    }

    /// <summary>텍스트 추론기을 비동기 열거자 형태로 반환합니다.</summary>
    /// <param name="cancellationToken">텍스트 추론을 취소할 취소 토큰입니다.</param>
    /// <returns>비동기 열거자 형태의 텍스트 추론기입니다.</returns>
    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        while (await Task.Run(NextText)) {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Text;
        }
    }
}