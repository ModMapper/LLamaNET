## LLamaNET
**.NET 7 (Test version)**
**Required llamacpp library**

***Example***
```C#
        const string prompt = "Hello, World!";
        LLamaParams param = new() {
            BatchSize = 512,
            ContextLength = 4096,
            GPULayerCount = 128,
        };
        using var model = LLMModel.FromFile("model.bin", param);
        TopSampler sampler = new() {
            RepeatPenalty = 1.1f,
            PresencePenalty = 0f,
            FrequencyPenalty = 1f,
            TopK = 40,
            TopP = 0.95f,
            Temperature = 0.8f,
        };
        using var inferencer = model.CreateInferencer(sampler);
        inferencer.Session.Write(prompt, true);
        foreach (var text in inferencer.InferenceText(500)) {
            Console.Write(text.Replace("\r", "\r\n"));
        }
```

***Samplers***
- ``TopSampler``
- ``MirostatSampler``
- ``MirostatV2Sampler``
- ``GrammarSampler``
