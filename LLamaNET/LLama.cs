namespace LLamaNET;

using LLamaNET.Native;

public static class LLama {
    public delegate void LogCallback(LLamaLogLevel logLevel, string text);
    private static NativeFunctions.LLamaLogCallback? logcallback;

    static LLama() {
        //NativeFunctions.llama_backend_init(true);
        //AppDomain.CurrentDomain.DomainUnload += (s, e) => NativeFunctions.llama_backend_free();
    }

    public static int MaxDevices { get; } = NativeFunctions.llama_max_devices();

    public static bool MMapSupported { get; } = NativeFunctions.llama_mmap_supported();

    public static bool MLockSupported { get; } = NativeFunctions.llama_mlock_supported();

    public static void SetLogCallback(LogCallback callback) {
        logcallback = (level, text, data) => callback(level, text);
        NativeFunctions.llama_log_set(logcallback, IntPtr.Zero);
    }

    public static string PrintSystemInfo()
        => NativeFunctions.llama_print_system_info();
}
