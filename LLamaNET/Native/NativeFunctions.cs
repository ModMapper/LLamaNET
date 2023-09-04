#pragma warning disable SYSLIB1054
#pragma warning disable CA2101
namespace LLamaNET.Native;
using System;
using System.Runtime.InteropServices;

public static unsafe class NativeFunctions {
    /// <summary>로그 출력 함수를 설정합니다. 기본 값은 std:err입니다.</summary>
    /// <param name="log_callback">로그 출력시 실행 될 함수입니다.</param>
    /// <param name="user_data">함수에 전달할 값입니다.</param>
    [DllImport("llama")]
    public static extern void llama_log_set(LLamaLogCallback? log_callback, IntPtr user_data);

    /// <summary>사용 가능한 디바이스 수를 가져옵니다.</summary>
    /// <returns>사용 가능한 디바이스 수 최대치 입니다. LLAMA_MAX_DEVICES값과 동일합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_max_devices();

    /// <summary>콘텍스트의 기본 설정 값을 가져옵니다.</summary>
    /// <returns>콘텍스트의 기본 설정 값입니다.</returns>
    [DllImport("llama")]
    public static extern LLamaContextParams llama_context_default_params();

    /// <summary>양자화의 기본 설정 값을 가져옵니다.</summary>
    /// <returns>양자화의 기본 설정 값입니다.</returns>
    [DllImport("llama")]
    public static extern LLamaModelQuantizeParams llama_model_quantize_default_params();

    /// <summary>mmap의 지원 여부를 반환합니다.</summary>
    /// <returns>mmap의 지원 여부입니다.</returns>
    [DllImport("llama")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool llama_mmap_supported();

    /// <summary>mlock의 지원 여부를 반환합니다.</summary>
    /// <returns>mlock의 지원 여부입니다.</returns>
    [DllImport("llama")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool llama_mlock_supported();

    /// <summary>LLamaCpp를 초기화합니다.</summary>
    /// <param name="numa">NUMA 시스템 사용 여부입니다.</param>
    [DllImport("llama")]
    public static extern void llama_backend_init([MarshalAs(UnmanagedType.Bool)] bool numa);
    /// <summary>LLamaCpp를 사용 해제합니다.</summary>
    [DllImport("llama")]
    public static extern void llama_backend_free();

    /// <summary>마이크로초 단위 시간을 가져옵니다.</summary>
    /// <returns>현재 시간입니다.</returns>
    [DllImport("llama")]
    public static extern long llama_time_us();

    /// <summary>파일로부터 모델을 불러옵니다.</summary>
    /// <param name="path_model">모델의 파일 위치입니다.</param>
    /// <param name="params">모델에 사용될 파라미터 값입니다.</param>
    /// <returns>모델의 핸들 값입니다. 0인 경우 실패입니다.</returns>
    [DllImport("llama")]
    public static extern LLamaModelHandle llama_load_model_from_file(string path_model, LLamaContextParams @params);

    /// <summary>모델 리소스를 모두 해제합니다.</summary>
    /// <param name="model">해제 할 모델의 핸들 값입니다.</param>
    [DllImport("llama")]
    public static extern void llama_free_model(LLamaModelHandle model);

    /// <summary>모델로부터 새 컨텍스트를 생성합니다.</summary>
    /// <param name="model">생성할 모델의 핸들 값입니다.</param>
    /// <param name="params">컨텍스트 생성에 사용될 파라미터 값입니다.</param>
    /// <returns>컨텍스트의 핸들 값입니다. 0인 경우 실패입니다.</returns>
    [DllImport("llama")]
    public static extern LLamaContextHandle llama_new_context_with_model(LLamaModelHandle model, LLamaContextParams @params);

    /// <summary>컨텍스트 리소스를 모두 해제합니다.</summary>
    /// <param name="ctx">해제 할 컨텍스트의 핸들 값입니다.</param>
    [DllImport("llama")]
    public static extern void llama_free(LLamaContextHandle ctx);

    /// <summary>모델을 양자화합니다.</summary>
    /// <param name="fname_inp">모델 파일의 입력 위치입니다.</param>
    /// <param name="fname_out">모델 파일의 출력 위치입니다.</param>
    /// <param name="params">양자화에 사용될 파라미터 값입니다.</param>
    /// <returns>성공한 경우 0을 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_model_quantize(string fname_inp, string fname_out, in LLamaModelQuantizeParams @params);

    /// <summary>모델에 LoRA를 적용합니다.</summary>
    /// <param name="model">LoRA를 적용할 모델의 핸들입니다.</param>
    /// <param name="path_lora">LoRA의 파일 위치입니다.</param>
    /// <param name="path_base_model">LoRA 베이스 모델의 위치입니다.</param>
    /// <param name="n_threads">동시 처리할 스레드의 수 입니다.</param>
    /// <returns>성공한 경우 0을 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_model_apply_lora_from_file(LLamaModelHandle model, string path_lora, string path_base_model, int n_threads);

    /// <summary>KV 캐시에 존재하는 토큰 수를 반환합니다.</summary>
    /// <param name="ctx">KV 캐시를 확인 할 컨텍스트 핸들입니다.</param>
    /// <returns>KV 캐시에 존재하는 토큰 수입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_get_kv_cache_token_count(LLamaContextHandle ctx);

    /// <summary>RNG에 랜덤 시드를 설정합니다.</summary>
    /// <param name="ctx">랜덤 시드를 설정 할 컨텍스트 핸들입니다.</param>
    /// <param name="seed">설정할 시드 값입니다.</param>
    [DllImport("llama")]
    public static extern void llama_set_rng_seed(LLamaContextHandle ctx, uint seed);

    /// <summary>상태 데이터 크기를 가져옵니다. (RNG, Logits, 임베딩, KV 캐시)</summary>
    /// <param name="ctx">상태 데이터를 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>상태 데이터의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern nint llama_get_state_size(LLamaContextHandle ctx);

    /// <summary>상태 데이터를 복사합니다. (RNG, Logits, 임베딩, KV 캐시)</summary>
    /// <param name="ctx">상태 데이터를 가져올 컨텍스트 핸들입니다.</param>
    /// <param name="dst">데이터가 복사될 위치입니다.</param>
    /// <returns>복사된 상태 데이터의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern nint llama_copy_state_data(LLamaContextHandle ctx, ref byte dst);

    /// <summary>상태 데이터를 설정합니다. (RNG, Logits, 임베딩, KV 캐시)</summary>
    /// <param name="ctx">상태 데이터를 설정할 컨텍스트 핸들입니다.</param>
    /// <param name="src">설정할 데이터의 위치입니다.</param>
    /// <returns>설정된 상태 데이터의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern nint llama_set_state_data(LLamaContextHandle ctx, ref byte src);

    /// <summary>파일로부터 세션을 불러옵니다.</summary>
    /// <param name="ctx">세션을 불러올 컨택스트 핸들입니다.</param>
    /// <param name="path_session">불러올 세션 파일의 경로입니다.</param>
    /// <param name="tokens_out">토큰 데이터가 저장될 위치입니다.</param>
    /// <param name="n_token_capacity">토근 저장 위치의 크기입니다.</param>
    /// <param name="n_token_count_out">저장된 토큰의 수 입니다.</param>
    /// <returns>세션 불러오기 성공 여부입니다.</returns>
    [DllImport("llama")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool llama_load_session_file(LLamaContextHandle ctx, string path_session, ref LLMToken tokens_out, nint n_token_capacity, out nint n_token_count_out);
    /// <summary>파일에 현재 세션을 저장합니다.</summary>
    /// <param name="ctx">세션을 저장할 컨택스트 핸들입니다.</param>
    /// <param name="path_session">저장될 세션 파일의 경로입니다.</param>
    /// <param name="tokens">현재 입력된 토큰 데이터의 위치입니다.</param>
    /// <param name="n_token_count">입력된 토큰의 갯수입니다.</param>
    /// <returns>세션 저장 성공 여부입니다.</returns>
    [DllImport("llama")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool llama_save_session_file(LLamaContextHandle ctx, string path_session, in LLMToken tokens, nint n_token_count);

    /// <summary>라마 추론을 평가하여 다음 토큰에 대한 Logit과 확률을 구합니다.</summary>
    /// <param name="ctx">토큰을 평가할 컨택스트 핸들입니다.</param>
    /// <param name="tokens">평가할 토큰 데이터의 위치입니다.</param>
    /// <param name="n_tokens">평가할 토큰의 수 입니다.</param>
    /// <param name="n_past">이전 호출에서 평가된 토큰 수 입니다.</param>
    /// <param name="n_threads">동시 처리할 스레드의 수 입니다.</param>
    /// <returns>성공한 경우 0을 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_eval(LLamaContextHandle ctx, in LLMToken tokens, int n_tokens, int n_past, int n_threads);

    /// <summary>라마 추론을 평가하여 다음 토큰에 대한 Logit과 확률을 구합니다.</summary>
    /// <param name="ctx">토큰을 평가할 컨택스트 핸들입니다.</param>
    /// <param name="embd">평가할 임베딩 데이터의 위치입니다.</param>
    /// <param name="n_tokens">평가할 토큰의 수 입니다.</param>
    /// <param name="n_past">이전 호출에서 평가된 토큰 수 입니다.</param>
    /// <param name="n_threads">동시 처리할 스레드의 수 입니다.</param>
    /// <returns>성공한 경우 0을 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_eval_embd(LLamaContextHandle ctx, in float embd, int n_tokens, int n_past, int n_threads);

    /// <summary>컨텍스트에 대해 계산하고 그래프를 내보냅니다. 이 함수는 테스트 용도입니다.</summary>
    /// <param name="ctx">토큰을 평가할 컨택스트 핸들입니다.</param>
    /// <param name="fname">그래프를 내보낼 파일 위치입니다.</param>
    /// <returns>성공한 경우 0을 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_eval_export(LLamaContextHandle ctx, string fname);

    /// <summary>제공된 텍스트를 토큰화합니다.</summary>
    /// <param name="ctx">토큰화를 진행할 컨텍스트 핸들입니다.</param>
    /// <param name="text">토큰화를 진행할 텍스트입니다.</param>
    /// <param name="tokens">출력될 토큰 데이터 위치입니다.</param>
    /// <param name="n_max_tokens">토큰 저장 위치의 크기입니다.</param>
    /// <param name="add_bos">BOS 토큰의 삽입 여부입니다.</param>
    /// <returns>성공 시 토큰 수를 반환하며, 실패 시에는 필요한 저장 크기를 음수로 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_tokenize(
        LLamaContextHandle ctx,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string text,
        ref LLMToken tokens,
        int n_max_tokens,
        [MarshalAs(UnmanagedType.Bool)] bool add_bos);

    /// <summary>제공된 텍스트를 토큰화합니다.</summary>
    /// <param name="model">토큰화를 진행할 모델 핸들입니다.</param>
    /// <param name="text">토큰화를 진행할 텍스트입니다.</param>
    /// <param name="tokens">출력될 토큰 데이터 위치입니다.</param>
    /// <param name="n_max_tokens">토큰 저장 위치의 크기입니다.</param>
    /// <param name="add_bos">BOS 토큰의 삽입 여부입니다.</param>
    /// <returns>성공 시 토큰 수를 반환하며, 실패 시에는 필요한 저장 크기를 음수로 반환합니다.</returns>
    [DllImport("llama")]
    public static extern int llama_tokenize_with_model(
        LLamaModelHandle model,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string text,
        ref LLMToken tokens,
        int n_max_tokens,
        [MarshalAs(UnmanagedType.Bool)] bool add_bos);

    /// <summary>컨텍스트의 Vocab의 크기를 가져옵니다.</summary>
    /// <param name="ctx">Vocab의 크기를 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>Vocab의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_vocab(LLamaContextHandle ctx);
    /// <summary>컨텍스트의 크기를 가져옵니다.</summary>
    /// <param name="ctx">컨텍스트의 크기를 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>컨텍스트의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_ctx(LLamaContextHandle ctx);
    /// <summary>컨텍스트의 임베딩 수를 가져옵니다.</summary>
    /// <param name="ctx">임베딩 수를 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>임베딩 수입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_embd(LLamaContextHandle ctx);

    /// <summary>모델의 Vocab의 크기를 가져옵니다.</summary>
    /// <param name="model">Vocab의 크기를 가져올 모델 핸들입니다.</param>
    /// <returns>Vocab의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_vocab_from_model(LLamaModelHandle model);
    /// <summary>모델의 컨텍스트 크기를 가져옵니다.</summary>
    /// <param name="model">컨텍스트의 크기를 가져올 모델 핸들입니다.</param>
    /// <returns>컨텍스트의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_ctx_from_model(LLamaModelHandle model);
    /// <summary>모델의 임베딩 수를 가져옵니다.</summary>
    /// <param name="model">임베딩 수를 가져올 모델 핸들입니다.</param>
    /// <returns>임베딩 수입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_n_embd_from_model(LLamaModelHandle model);

    /// <summary>모델 타입 데이터를 가져옵니다.</summary>
    /// <param name="model">타입을 가져올 모델의 핸들입니다.</param>
    /// <param name="buf">타입 데이터가 복사 될 위치입니다.</param>
    /// <param name="buf_size">타입 데이터 복사 위치의 크기입니다.</param>
    /// <returns>복사된 모델 타입의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_model_type(LLamaModelHandle model, ref byte buf, nint buf_size);

    /// <summary>컨텍스트의 Vocab을 가져옵니다.</summary>
    /// <param name="ctx">Vocab을 가져올 컨텍스트 핸들입니다.</param>
    /// <param name="strings">Vocab의 데이터가 저장될 문자열 데이터 위치입니다.</param>
    /// <param name="scores">스코어가 저장될 위치입니다.</param>
    /// <param name="capacity">문자열 데이터와 스코어의 저장 공간 크기입니다.</param>
    /// <returns>저장된 문자열 데이터와 스코어 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_get_vocab(LLamaContextHandle ctx, string[] strings, ref float scores, int capacity);

    /// <summary>모델의 Vocab을 가져옵니다.</summary>
    /// <param name="model">Vocab을 가져올 모델 핸들입니다.</param>
    /// <param name="strings">Vocab의 데이터가 저장될 문자열 데이터 위치입니다.</param>
    /// <param name="scores">스코어가 저장될 위치입니다.</param>
    /// <param name="capacity">문자열 데이터와 스코어의 저장 공간 크기입니다.</param>
    /// <returns>저장된 문자열 데이터와 스코어 크기입니다.</returns>
    [DllImport("llama")]
    public static extern int llama_get_vocab_from_model(LLamaModelHandle model, string[] strings, ref float scores, int capacity);

    /// <summary>컨텍스트의 Logit 참조 위치를 가져옵니다.</summary>
    /// <param name="ctx">Logit을 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>Logit의 참조 위치입니다. 토큰 수 * Vocab의 크기를 가집니다.</returns>
    [DllImport("llama")]
    public static extern float* llama_get_logits(LLamaContextHandle ctx);

    /// <summary>컨텍스트의 임베딩 참조 위치를 가져옵니다.</summary>
    /// <param name="ctx">임베딩을 가져올 컨텍스트 핸들입니다.</param>
    /// <returns>임베딜의 위치입니다. 임베딩 크기의 크기입니다.</returns>
    [DllImport("llama")]
    public static extern float* llama_get_embeddings(LLamaContextHandle ctx);

    /// <summary>해당 토큰에 대한 문자열을 가져옵니다.</summary>
    /// <param name="ctx">토큰에 대한 정보가 들어간 컨텍스트 핸들입니다.</param>
    /// <param name="token">문자열을 가져올 토큰입니다.</param>
    /// <returns>해당 토큰에 대한 문자열입니다.</returns>
    [DllImport("llama")]
    public static extern byte* llama_token_to_str(LLamaContextHandle ctx, LLMToken token);

    /// <summary>해당 토큰에 대한 문자열을 가져옵니다.</summary>
    /// <param name="model">토큰에 대한 정보가 들어간 모델 핸들입니다.</param>
    /// <param name="token">문자열을 가져올 토큰입니다.</param>
    /// <returns>해당 토큰에 대한 문자열입니다.</returns>
    [DllImport("llama")]
    public static extern byte* llama_token_to_str_with_model(LLamaModelHandle model, LLMToken token);

    /// <summary>문장 시작 토큰을 가져옵니다.</summary>
    /// <returns>문장의 시작 토큰입니다.</returns>
    [DllImport("llama")]
    public static extern LLMToken llama_token_bos();  // beginning-of-sentence
    /// <summary>문장 종료 토큰을 가져옵니다.</summary>
    /// <returns>문장의 종료 토큰입니다.</returns>
    [DllImport("llama")]
    public static extern LLMToken llama_token_eos();  // end-of-sentence
    /// <summary>새 줄을 알리는 토큰을 가져옵니다.</summary>
    /// <returns>새 줄을 알리는 토큰입니다.</returns>
    [DllImport("llama")]
    public static extern LLMToken llama_token_nl();   // next-line

    [DllImport("llama")]
    public static extern LLamaGrammarHandle llama_grammar_init(LLamaGrammarElement** rules, nint n_rules, nint start_rule_index);

    [DllImport("llama")]
    public static extern void llama_grammar_free(LLamaGrammarHandle grammar);

    [DllImport("llama")]
    public static extern void llama_sample_repetition_penalty(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, in LLMToken last_tokens, nint last_tokens_size, float penalty);

    [DllImport("llama")]
    public static extern void llama_sample_frequency_and_presence_penalties(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, in LLMToken last_tokens, nint last_tokens_size, float alpha_frequency, float alpha_presence);

    [DllImport("llama")]
    public static extern void llama_sample_classifier_free_guidance(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, LLamaContextHandle guidance_ctx, float scale);

    [DllImport("llama")]
    public static extern void llama_sample_softmax(LLamaContextHandle ctx, in LLamaTokenDataArray candidates);

    [DllImport("llama")]
    public static extern void llama_sample_top_k(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, int k, nint min_keep);

    [DllImport("llama")]
    public static extern void llama_sample_top_p(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float p, nint min_keep);

    [DllImport("llama")]
    public static extern void llama_sample_tail_free(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float z, nint min_keep);

    [DllImport("llama")]
    public static extern void llama_sample_typical(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float p, nint min_keep);
    [DllImport("llama")]
    public static extern void llama_sample_temperature(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float temp);

    [DllImport("llama")]
    public static extern void llama_sample_grammar(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, LLamaGrammarHandle grammar);

    [DllImport("llama")]
    public static extern LLMToken llama_sample_token_mirostat(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float tau, float eta, int m, in float mu);

    [DllImport("llama")]
    public static extern LLMToken llama_sample_token_mirostat_v2(LLamaContextHandle ctx, in LLamaTokenDataArray candidates, float tau, float eta, in float mu);

    [DllImport("llama")]
    public static extern LLMToken llama_sample_token_greedy(LLamaContextHandle ctx, in LLamaTokenDataArray candidates);

    [DllImport("llama")]
    public static extern LLMToken llama_sample_token(LLamaContextHandle ctx, in LLamaTokenDataArray candidates);

    [DllImport("llama")]
    public static extern void llama_grammar_accept_token(LLamaContextHandle ctx, LLamaGrammarHandle grammar, LLMToken token);

    /// <summary>성능 타이밍 정보를 가져옵니다.</summary>
    /// <param name="ctx">정보를 가져올 컨텍스트입니다.</param>
    /// <returns>가져온 타이밍 정보입니다.</returns>
    [DllImport("llama")]
    public static extern LLamaTimings llama_get_timings(LLamaContextHandle ctx);
    /// <summary>성능 타이밍 정보를 출력합니다.</summary>
    /// <param name="ctx">정보를 가져올 컨텍스트입니다.</param>
    [DllImport("llama")]
    public static extern void llama_print_timings(LLamaContextHandle ctx);
    /// <summary>성능 타이밍 정보를 초기화합니다.</summary>
    /// <param name="ctx">정보를 초기화할 컨텍스트입니다.</param>
    [DllImport("llama")]
    public static extern void llama_reset_timings(LLamaContextHandle ctx);

    /// <summary>시스템 정보에 관한 문자열을 출력합니다.</summary>
    /// <returns>시스템 정보가 포함된 문자열입니다.</returns>
    [DllImport("llama")]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static extern string llama_print_system_info();

    public delegate void LLamaLogCallback(LLamaLogLevel level, string text, IntPtr user_data);
}