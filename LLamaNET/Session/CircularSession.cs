namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;
using System.IO;
using System.Runtime.InteropServices;

public class CircularSession : LLMSession {
    private readonly LLMToken[] buffer;
    private readonly int contextsize;
    private int index;

    public CircularSession(LLMContext context) : this((LLamaContext)context, context.BatchSize) { }

    public CircularSession(LLamaContext context, int batchSize) : base(context, batchSize, false) {
        contextsize = context.ContextSize;
        buffer = new LLMToken[contextsize + BatchSize];
    }

    public CircularSession(LLamaContext context, int batchSize, bool owned) : base(context, batchSize, owned) {
        contextsize = context.ContextSize;
        buffer = new LLMToken[contextsize + BatchSize];
    }

    public override int Length => Math.Min(index, contextsize);

    public override ReadOnlySpan<LLMToken> Span
        => buffer.AsSpan()[Math.Max(index - contextsize, 0)..index];

    public override ReadOnlyMemory<LLMToken> Memory
        => buffer.AsMemory()[Math.Max(index - contextsize, 0)..index];

    public override void Add(LLMToken token) {
        if (index == buffer.Length) {
            buffer.AsSpan(BatchSize + 1).CopyTo(buffer);
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
                buffer.AsSpan(BatchSize + tokens.Length).CopyTo(buffer);
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

    protected override void ReadTokens(Stream stream, int count)
        => stream.ReadExactly(MemoryMarshal.AsBytes(buffer.AsSpan(0, index = count)));

    protected override void WriteTokens(Stream stream)
        => stream.Write(MemoryMarshal.AsBytes(buffer.AsSpan()[Math.Max(index - contextsize, 0)..index]));
}
