using LLamaNET;
using LLamaNET.Sampler;

internal class Program {
    private static void Main(string[] args) {
        const string filepath = @"E:\작업\AI 모델\Text\OpenOrca\openorca-platypus2-13b.ggmlv3.q4_1.bin";
        LLamaParams param = new() {
            BatchSize = 512,
            ContextLength = 4096,
            GPULayerCount = 128,
        };

        const string InputPrefix = "\n\n### Instruction:\n\n";
        const string OutputPrefix = "\n\n### Response:\n\n";

        using var model = LLMModel.FromFile(filepath, param);
        using var context = model.CreateContext();
        TopSampler sampler = new() {
            RepeatPenalty = 1.1f,
            PresencePenalty = 0f,
            FrequencyPenalty = 1f,
            TopK = 40,
            TopP = 0.95f,
            Temperature = 0.8f,
        };
        LLama.SetLogCallback((v, x) => { });

        var inferencer = context.CreateInferencer(sampler);
        var prompt = inferencer.Session;
        inferencer.AntiPrompt = "###";

        prompt.Append(" ", true);
        prompt.Append(InputPrefix);
        Console.Write(InputPrefix);

        while (true) {
            string text = Console.ReadLine() ?? string.Empty;
            if (text == "exit") return;
            prompt.Append(text);
            prompt.AppendEOS();

            prompt.Append(OutputPrefix);
            Console.Write(OutputPrefix);

            foreach(var word in inferencer.InferenceText(500)) {
                Console.Write(word.Replace("\r", "\r\n"));
            }
            prompt.Append(InputPrefix);
            Console.Write(InputPrefix);
        }
    }
}