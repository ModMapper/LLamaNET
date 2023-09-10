namespace LLamaNET;

using LLamaNET.Inferencer;
using LLamaNET.LLamaCpp;
using LLamaNET.Sampler;
using LLamaNET.Session;

using System;
using System.Text;

/// <summary>토큰에 대한 추론을 진행하는 토큰 추론기입니다.</summary>
public class LLMInferencer : IDisposable {
    /// <summary>기본 샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    public LLMInferencer(LLMSession session)
        => (Session, Sampler) = (session, new TopSampler());

    /// <summary>샘플러를 사용하여 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public LLMInferencer(LLMSession session, LLMSampler sampler)
        => (Session, Sampler) = (session, sampler);

    /// <summary>해당 세션에 대한 토큰 추론기를 생성합니다.</summary>
    /// <param name="session">토큰 추론을 진행 할 세션입니다.</param>
    /// <param name="sampler">토큰 추론에 사용할 샘플러입니다.</param>
    public LLMInferencer(LLMContext context, LLMSampler sampler)
    {
        Session = new CircularSession((LLamaContext)context, context.BatchSize);
        Sampler = sampler;
    }

    /// <summary>세션의 리소스를 해제합니다.</summary>
    public void Dispose() {
        Session.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>추론이 진행될 세션입니다.</summary>
    public LLMSession Session { get; }

    /// <summary>내부 컨텍스트입니다.</summary>
    protected LLamaContext Context => (LLamaContext)Session;

    /// <summary>토큰 샘플링에 사용할 샘플러입니다.</summary>
    public LLMSampler Sampler { get; set; }

    /// <summary>연산을 진행할 배치 크기입니다.</summary>
    public int BatchSize => Session.BatchSize;

    /// <summary>토큰 생성을 종료할 종료자입니다.</summary>
    public string AntiPrompt { get; set; } = string.Empty;

    /// <summary>생성에 사용할 스레드의 갯수입니다.</summary>
    public int Threads { get; set; } = LLama.MaxDevices == 1 ? Environment.ProcessorCount : 1;

    /// <summary>토큰 추론기를 가져옵니다.</summary>
    /// <returns>토큰 추론기입니다.</returns>
    public TokenInferencer Inference()
        => TokenInferencer.CreateInferencer(Session, Sampler, BatchSize, Threads);

    /// <summary>해당 토큰 수 만큼 추론하는 토큰 추론기를 가져옵니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <returns>토큰 추론기입니다.</returns>
    public TokenInferencer Inference(int count) {
        var inferencer = TokenInferencer.CreateInferencer(Session, Sampler, BatchSize, Threads);
        inferencer.MaxTokens = count;
        return inferencer;
    }

    /// <summary>토큰 추론기를 비동기적으로 가져옵니다.</summary>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>토큰 추론기입니다.</returns>
    public async Task<TokenInferencer> InferenceAsync(CancellationToken token = default)
        => await TokenInferencer.CreateInferencerAsync(Session, Sampler, BatchSize, Threads, token);

    /// <summary>해당 토큰 수 만큼 추론하는 토큰 추론기를 비동기적으로 가져옵니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>토큰 추론기입니다.</returns>
    public async Task<TokenInferencer> InferenceAsync(int count, CancellationToken token = default) {
        var inferencer = await TokenInferencer.CreateInferencerAsync(Session, Sampler, BatchSize , Threads, token);
        inferencer.MaxTokens = count;
        return inferencer;
    }

    /// <summary>텍스트 추론기를 가져옵니다.</summary>
    /// <returns>텍스트 추론기입니다.</returns>
    public TextInferencer InferenceText()
        => new(Inference(), AntiPrompt);

    /// <summary>해당 토큰 수 만큼 추론하는 텍스트 추론기를 가져옵니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <returns>텍스트 추론기입니다.</returns>
    public TextInferencer InferenceText(int count)
        => new(Inference(count), AntiPrompt);

    /// <summary>해당 토큰 수 만큼 추론하는 텍스트 추론기를 가져옵니다.</summary>
    /// <param name="antiPrompt">토큰 생성을 종료할 종료자입니다.</param>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <returns>텍스트 추론기입니다.</returns>
    public TextInferencer InferenceText(string antiPrompt, int count)
        => new(Inference(count), antiPrompt);

    /// <summary>해당 토큰 수 만큼 텍스트를 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <returns>추론한 텍스트입니다.</returns>
    public string InferenceAll(int count) {
        TextInferencer inferencer = new(Inference(count), AntiPrompt);
        StringBuilder sb = new();
        while(inferencer.NextText())
            sb.Append(inferencer.Text);
        return sb.ToString();
    }

    /// <summary>텍스트 추론기를 비동기적으로 가져옵니다.</summary>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>텍스트 추론기입니다.</returns>
    public async Task<TextInferencer> InferenceTextAsync(CancellationToken token = default)
        => new(await InferenceAsync(token), AntiPrompt);

    /// <summary>해당 토큰 수 만큼 추론하는 텍스트 추론기를 비동기적으로 가져옵니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>텍스트 추론기입니다.</returns>
    public async Task<TextInferencer> InferenceTextAsync(int count, CancellationToken token = default)
        => new(await InferenceAsync(count, token), AntiPrompt);

    /// <summary>해당 토큰 수 만큼 추론하는 텍스트 추론기를 비동기적으로 가져옵니다.</summary>
    /// <param name="antiPrompt">토큰 생성을 종료할 종료자입니다.</param>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>텍스트 추론기입니다.</returns>
    public async Task<TextInferencer> InferenceTextAsync(string antiPrompt, int count, CancellationToken token = default)
        => new(await InferenceAsync(count, token), antiPrompt);

    /// <summary>해당 토큰 수 만큼 텍스트를 비동기적으로 추론합니다.</summary>
    /// <param name="count">추론할 토큰의 수 입니다.</param>
    /// <param name="token">토큰 생성을 종료할 종료자 토큰입니다.</param>
    /// <returns>추론한 텍스트입니다.</returns>
    public async Task<string> InferenceAllAsync(int count, CancellationToken token = default) {
        TextInferencer inferencer = new(await InferenceAsync(count, token), AntiPrompt);
        StringBuilder sb = new();
        while (await Task.Run(inferencer.NextText)) {
            token.ThrowIfCancellationRequested();
            sb.Append(inferencer.Text);
        }
        return sb.ToString();
    }
}
