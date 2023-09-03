namespace LLamaNET.Context;

using LLamaNET.LLamaCpp;

using System;
using System.Collections.Generic;

public class LoopTokens : LLMTokens {
    private readonly LLMToken[] buffer;
    private readonly int contextsize, buffersize;
    private int index;

    public LoopTokens(LLamaContext context, int buffersize) : base(context) {
        if (buffersize < 0) throw new ArgumentOutOfRangeException(nameof(buffersize));
        if (buffersize < 0x100) buffersize = 0x100;
        contextsize = Context.ContextSize;
        this.buffersize = buffersize;
        buffer = new LLMToken[contextsize + buffersize];
    }

    public override int Length => Math.Min(index, contextsize);

    protected override Span<LLMToken> Span
        => buffer.AsSpan()[Math.Max(index - Context.ContextSize, 0)..index];

    public override void Add(LLMToken token) {
        if(index == buffer.Length) {
            buffer.AsSpan(buffersize + 1).CopyTo(buffer);
            buffer[contextsize - 1] = token;
            index = contextsize;
        } else {
            buffer[index++] = token;
        }
    }

    public override void AddRange(Span<LLMToken> tokens) {
        if(buffer.Length <= index + tokens.Length) {
            buffer.AsSpan(buffersize + tokens.Length).CopyTo(buffer);
            tokens.CopyTo(buffer.AsSpan(contextsize - tokens.Length));
            index = contextsize;
        } else {
            tokens.CopyTo(buffer.AsSpan(index));
            index += tokens.Length;
        }
    }

    public override void AddRange(ICollection<LLMToken> tokens) {
        if (buffer.Length <= index + tokens.Count) {
            buffer.AsSpan(buffersize + tokens.Count).CopyTo(buffer);
            tokens.CopyTo(buffer, contextsize - tokens.Count);
            index = contextsize;
        } else {
            tokens.CopyTo(buffer, index);
            index += tokens.Count;
        }
    }

    public override void Clear()
        => index = 0;
}
