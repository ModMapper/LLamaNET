## LLamaNET
**.NET 7 (Test version)**

***Example***
```C#
        LLamaParams param = new() {
            BatchSize = 512,
            ContextLength = 4096,
            GPULayerCount = 128,
        };
        using var model = LLMModel.FromFile("model.bin", param);
        using var context = model.CreateContext();
        TopSampler sampler = new() {
            RepeatPenalty = 1.1f,
            PresencePenalty = 0f,
            FrequencyPenalty = 1f,
            TopK = 40,
            TopP = 0.95f,
            Temperature = 0.8f,
        };
        var inferencer = context.CreateInferencer(sampler);
        inferencer.Session.Append(prompt);
        foreach(var text in inferencer.InferenceText(500)) {
            Console.Write(text.Replace("\r", "\r\n"));
        }
```

***Samplers***
- ``TopSampler``
- ``MirostatSampler``
- ``MirostatV2Sampler``
- ``GrammarSampler``
