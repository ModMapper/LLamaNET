namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;

public class CircularSession : LLMSession {
    private readonly LLMToken[] buffer;
    private readonly int buffersize;
    private readonly int contextsize;
    private int index;

    public CircularSession(LLamaContext context, int buffersize) : base(context) {
        if (buffersize < 0) throw new ArgumentOutOfRangeException(nameof(buffersize));
        contextsize = context.ContextSize;
        this.buffersize = Math.Min(0x100, buffersize);
        buffer = new LLMToken[contextsize + this.buffersize];
    }

    public override int Length => Math.Min(index, contextsize);

    protected override ReadOnlySpan<LLMToken> Span
        => buffer.AsSpan()[Math.Max(index - contextsize, 0)..index];

    public override void Add(LLMToken token) {
        if (index == buffer.Length) {
            buffer.AsSpan(buffersize + 1).CopyTo(buffer);
            buffer[contextsize - 1] = token;
            index = contextsize;
        } else {
            buffer[index++] = token;
        }
    }

    public override void AddRange(ReadOnlySpan<LLMToken> tokens) {
        if (buffer.Length <= index + tokens.Length) {
            if (contextsize <= tokens.Length) {
                tokens[^contextsize..].CopyTo(buffer);
            } else {
                buffer.AsSpan(buffersize + tokens.Length).CopyTo(buffer);
                tokens.CopyTo(buffer.AsSpan(contextsize - tokens.Length));
            }
            index = contextsize;
        } else {
            tokens.CopyTo(buffer.AsSpan(index));
            index += tokens.Length;
        }
    }

    public override void Clear()
        => index = 0;
}
