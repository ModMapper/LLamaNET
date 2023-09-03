namespace LLamaNET.Context;

using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;

public class FixedTokens : LLMTokens {
    private readonly LLMToken[] buffer;
    private int index;

    public FixedTokens(LLamaContext context) : base(context) {
        buffer = new LLMToken[Context.ContextSize];
    }

    public override int Length => index;

    protected override Span<LLMToken> Span
        => buffer.AsSpan(0, index);

    public override void Add(LLMToken token) {
        if (buffer.Length == index)
            throw new InternalBufferOverflowException();
        buffer[index++] = token;
    }

    public override void AddRange(Span<LLMToken> tokens) {
        if(buffer.Length < index + tokens.Length)
            throw new InternalBufferOverflowException();
        tokens.CopyTo(buffer.AsSpan(index));
        index += tokens.Length;
    }

    public override void AddRange(ICollection<LLMToken> tokens) {
        if (buffer.Length < index + tokens.Count)
            throw new InternalBufferOverflowException();
        tokens.CopyTo(buffer, index);
        index += tokens.Count;
    }

    public override void Clear()
        => index = 0;
}
