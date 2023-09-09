namespace LLamaNET.Session;

using LLamaNET.LLamaCpp;

using System;
using System.IO;
using System.Runtime.InteropServices;

public class FixedSession : LLMSession {
    private readonly LLMToken[] buffer;
    private int index;

    public FixedSession(LLMContext context) : this((LLamaContext)context, context.BatchSize) {}

    public FixedSession(LLamaContext context, int batchSize) : base(context, batchSize, false)
        => buffer = new LLMToken[context.ContextSize];

    public FixedSession(LLamaContext context, int batchSize, bool owned) : base(context, batchSize, owned)
        => buffer = new LLMToken[context.ContextSize];

    public override int Length => index;

    /// <summary>해당 인덱스에 위치한 토큰을 가져오거나 설정합니다.</summary>
    /// <param name="index">가져오거나 설정할 토큰의 인덱스입니다.</param>
    /// <returns>해당 인덱스에 위치한 토큰입니다.</returns>
    public LLMToken this[int index] {
        get {
            if(index < 0 || index <= this.index)
                throw new IndexOutOfRangeException();
            return buffer[index];
        }
        set {
            if (index < 0 || index <= this.index)
                throw new IndexOutOfRangeException();
            buffer[index] = value;
        }
    }

    public override ReadOnlySpan<LLMToken> Span
        => buffer.AsSpan(0, index);

    public override ReadOnlyMemory<LLMToken> Memory
        => buffer.AsMemory(0, index);

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

    protected override void ReadTokens(Stream stream, int count)
        => stream.ReadExactly(MemoryMarshal.AsBytes(buffer.AsSpan(0, index = count)));

    protected override void WriteTokens(Stream stream)
        => stream.Write(MemoryMarshal.AsBytes(buffer.AsSpan(0, index)));
}
