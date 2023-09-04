namespace LLamaNET;

using LLamaNET.Native;

/// <summary>LLama</summary>
public static class LLama {
    public delegate void LogCallback(LLamaLogLevel logLevel, string text);
    private static NativeFunctions.LLamaLogCallback? logcallback;

    static LLama() {
        //NativeFunctions.llama_backend_init(true);
        //AppDomain.CurrentDomain.DomainUnload += (s, e) => NativeFunctions.llama_backend_free();
    }

    /// <summary>최대 사용 가능한 디바이스 수를 가져옵니다.</summary>
    public static int MaxDevices { get; } = NativeFunctions.llama_max_devices();

    /// <summary>MMap의 사용 가능 여부를 가져옵니다.</summary>
    public static bool MMapSupported { get; } = NativeFunctions.llama_mmap_supported();

    /// <summary>MLock의 사용 가능 여부를 가져옵니다.</summary>
    public static bool MLockSupported { get; } = NativeFunctions.llama_mlock_supported();

    /// <summary>라마 로그 콜백을 설정합니다.</summary>
    /// <param name="callback">로그 콜백입니다.</param>
    public static void SetLogCallback(LogCallback? callback) {
        logcallback = callback == null ? null :
            (level, text, data) => callback(level, text);
        NativeFunctions.llama_log_set(logcallback, IntPtr.Zero);
    }

    /// <summary>시스템 정보를 출력합니다.</summary>
    /// <returns>시스템 정보에 관한 문자열입니다.</returns>
    public static string PrintSystemInfo()
        => NativeFunctions.llama_print_system_info();
}
