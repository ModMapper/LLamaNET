namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;

public class FixedSession : LLMSession {
    private readonly LLMToken[] buffer;
    private int index;

    public FixedSession(LLamaContext context) : base(context)
        => buffer = new LLMToken[context.ContextSize];

    public override int Length => index;

    protected override ReadOnlySpan<LLMToken> Span
        => buffer.AsSpan(0, index);

    public override void Add(LLMToken token) {
        if (buffer.Length == index)
            throw new InternalBufferOverflowException();
        buffer[index++] = token;
    }

    public override void AddRange(ReadOnlySpan<LLMToken> tokens) {
        if (buffer.Length < index + tokens.Length)
            throw new InternalBufferOverflowException();
        tokens.CopyTo(buffer.AsSpan(index));
        index += tokens.Length;
    }

    public override void Clear()
        => index = 0;
}
