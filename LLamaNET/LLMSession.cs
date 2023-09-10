namespace LLamaNET;
using LLamaNET.LLamaCpp;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>LLM 세션입니다.</summary>
public abstract class LLMSession : IDisposable {
    private const int MaxBatchSize = 0x200;
    private readonly bool owned;

    /// <summary>컨텍스트로부터 새 세션을 생성합니다.</summary>
    /// <param name="context">새 세션을 생성할 컨텍스트입니다.</param>
    /// <param name="batchSize">토큰 계산을 진행할 배치 크기입니다.</param>
    /// <param name="owned">해당 컨텍스트의 소유 여부입니다.</param>
    public LLMSession(LLamaContext context, int batchSize, bool owned) {
        if (batchSize < 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
        if (batchSize == 0) batchSize = 0x100;
        BatchSize = Math.Min(batchSize, 0x200);
        Context = context;
        this.owned = owned;
    }

    /// <summary>컨텍스트를 소유한 경우 컨텍스트 리소스를 해제합니다.</summary>
    ~LLMSession() => Dispose(disposing: false);

    /// <summary>컨텍스트를 소유한 경우 컨텍스트 리소스를 해제합니다.</summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing) {
        if (owned) Context.Dispose();
    }

    /// <summary>컨텍스트를 소유한 경우 컨텍스트 리소스를 해제합니다.</summary>
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>내부 토크나이저입니다.</summary>
    protected LLamaContext Context { get; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize { get; }

    /// <summary>동기 추론에 사용되는 토큰 버퍼 <see cref="Span{LLMToken}"/>입니다.</summary>
    public abstract ReadOnlySpan<LLMToken> Span { get; }

    /// <summary>비동기 추론에 사용되는 토큰 버퍼 <see cref="Memory{LLMToken}"/>입니다.</summary>
    public abstract ReadOnlyMemory<LLMToken> Memory { get; }

    /// <summary>현재 세션의 토큰 갯수를 가져옵니다.</summary>
    public abstract int Length { get; }

    /// <summary>해당 세션의 최대 콘텍스트 길이를 가져옵니다.</summary>
    public int ContextSize => Context.ContextSize;

    /// <summary>세션에 토큰을 추가합니다.</summary>
    /// <param name="token">세션에 추가할 토큰입니다.</param>
    public abstract void Add(LLMToken token);

    /// <summary>세션에 토큰 <see cref="Span{LLMToken}"/>을 추가합니다.</summary>
    /// <param name="tokens">세션에 추가할 토큰 <see cref="Span{LLMToken}"/>입니다.</param>
    public virtual void AddRange(ReadOnlySpan<LLMToken> tokens) {
        foreach (LLMToken token in tokens)
            Add(token);
    }

    /// <summary>세션에 토큰 열거를 추가합니다.</summary>
    /// <param name="tokens">세션에 추가할 토큰 열거입니다.</param>
    public virtual void AddRange(IEnumerable<LLMToken> tokens) {
        foreach (LLMToken token in tokens)
            Add(token);
    }

    /// <summary>세션에 저장된 토큰을 삭제합니다.</summary>
    public abstract void Clear();

    /// <summary>주어진 크기가 남도록 앞부분을 자릅니다.</summary>
    /// <param name="count">남길 크기입니다.</param>
    public abstract void Cutoff(int count);

    /// <summary>세션에 텍스트를 작성합니다.</summary>
    /// <param name="text">세션에 작성할 텍스트입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string text)
        => Write(text, false);

    /// <summary>세션에 텍스트를 작성합니다.</summary>
    /// <param name="text">세션에 작성할 텍스트입니다.</param>
    /// <param name="bos">시작 토큰의 추가 여부입니다.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string text, bool bos) {
        int size = Math.Abs(Context.Tokenize(text, stackalloc LLMToken[0x1], bos));
        if (size <= 0x1000) {
            Span<LLMToken> buffer = stackalloc LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange(buffer);
        } else {
            LLMToken[] buffer = new LLMToken[size];
            Context.Tokenize(text, buffer, bos);
            AddRange((IReadOnlyCollection<LLMToken>)buffer);
        }
    }

    /// <summary>세션에 시작 토큰을 추가합니다.</summary>
    public void WriteBOS()
        => Add(LLMToken.TokenBOS);

    /// <summary>세션에 종료 토큰을 추가합니다.</summary>
    public void WriteEOS()
        => Add(LLMToken.TokenEOS);

    /// <summary>스트림으로부터 토큰을 읽어들입니다.</summary>
    /// <param name="stream">토큰을 읽어들일 스트림입니다.</param>
    /// <param name="count">읽어들일 토큰의 수 입니다.</param>
    protected abstract void ReadTokens(Stream stream, int count);

    /// <summary>스트림에 토큰을 작성합니다.</summary>
    /// <param name="stream">토큰을 작성할 스트림입니다.</param>
    protected abstract void WriteTokens(Stream stream);

    private static readonly byte[] Magic = new byte[] { 0x67, 0x67, 0x73, 0x6E, 0x01, 0x00, 0x00, 0x00 };

    /// <summary>스트림으로부터 세션을 불러옵니다.</summary>
    /// <param name="stream">세션을 불러올 스트림입니다.</param>
    /// <returns>불러오기의 성공 여부입니다.</returns>
    public bool LoadSession(Stream stream) {
        try {
            {   // Magic / Version
                Span<byte> buffer = stackalloc byte[Magic.Length];
                stream.ReadExactly(buffer);
                if (!buffer.SequenceEqual(Magic)) return false;
            }
            {   // Context Data
                Span<byte> buffer = stackalloc byte[(int)Context.GetStateSize()];
                stream.ReadExactly(buffer);
                Context.SetStateData(buffer);
            }
            {   // Token Data
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                stream.ReadExactly(buffer);
                ReadTokens(stream, BinaryPrimitives.ReadInt32LittleEndian(buffer));
            }
            return true;
        } catch {
            return false;
        }
    }

    /// <summary>스트림에 세션을 저장합니다.</summary>
    /// <param name="stream">세션을 저장할 스트림입니다.</param>
    public void Save(Stream stream) {
        // Magic / Version
        stream.Write(Magic);
        {   // Context Data
            Span<byte> buffer = stackalloc byte[(int)Context.GetStateSize()];
            Context.GetStateData(buffer);
            stream.Write(buffer);
        }
        {   // Token Data
            int length = Length;
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, length);
            stream.Write(buffer);
            WriteTokens(stream);
        }
    }

    /// <summary>세션 내부의 <see cref="LLamaContext"/>를 가져옵니다.</summary>
    /// <param name="session"><see cref="LLamaContext"/>를 가져올 세션입니다.</param>
    public static explicit operator LLamaContext(LLMSession session)
        => session.Context;
}
