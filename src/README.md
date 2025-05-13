# Semantic Kernel examples

A bunch of examples that use [Microsoft's Semantic Kernel](https://github.com/microsoft/semantic-kernel/tree/main)

## Prerequisites

- Any computer that is able to run C# .NET 9
- [Ollama](https://ollama.com/) installed.
- A few models need to be downloaded:
    ```bash
    ollama pull gemma3:4b
    ollama pull minicpm-v
    ollama pull llama3.1:8b
    ollama pull mxbai-embed-large
    ```
- Enough VRAM/RAM for the GPU/CPU. Approx 8GB

The examples are intended to demonstrate and experiment with the Semantic Kernel library. And thus the models are kept as lightweight as possible.

### References

Presentation: https://sk.netwatwezoeken.nl/

`issues.csv` in the EmbeddingsAndVectorStore example taken from: https://huggingface.co/datasets/giseldo/neodataset
`issues-70.csv` and `issues-30.csv` together are the same set randomly selected in to a 70% and 30% part.