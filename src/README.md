# Semantic Kernel examples

A bunch of examples that use [Microsoft's Semantic Kernel](https://github.com/microsoft/semantic-kernel/tree/main)

## The examples

### 01 Basic Chat
A fundamental chat implementation demonstrating the core chat functionality.

### 02 Chat History
Extends the basic chat with history to have a conversation

### 03 Templating
Shows how templates ([Prompty.ai](https://prompty.ai/)) can be used.

### 04 Functions
Demonstrates how to use tools. A way to have the LLM call your code.

### 05 MCP
Demonstrates how to use MCP. A way to have the LLM call your other software through MCP.

### 06 Agent Framework
Implementation showcasing AI agent orchestration

### 07 RAG
Retrieval Augmented Generation (RAG)

## Prerequisites

- Any computer that is able to run C# .NET 9
- Enough RAM for the GPU/CPU. Approx 8GB

The examples are intended to demonstrate and experiment with the Semantic Kernel library. And thus the models are kept as lightweight as possible.

## Running with .NET Aspire (recommended)

The solution includes an Aspire `AppHost` that orchestrates all dependencies for you:

- Starts an Ollama container and downloads the required models on first run.
- Starts Redis Stack with Redis Commander for inspection.
- Runs the `07-Rag` project in prepare mode (`--prepare`) to build embeddings and populate Redis so RAG is ready to go.
- Coordinates startup of the `WebUI` once all models are available.

How to run:
- From the command line: `dotnet run --project AppHost/AppHost.csproj`
- From the IDE: set `AppHost` as the startup project and run.

On first run, pulling the models can take a while. Subsequent runs are fast thanks to the persistent volumes.

Use the Aspire dashboard to discover see traces.

## Running the WebUI

Aspire is the remmomended way to run the examples. But yu can also run the WebUI project directly.

### Additional prerequisites
- Manual setup: install [Ollama](https://ollama.com/) and pull the required models yourself:
  ```bash
  ollama pull gemma3:4b
  ollama pull minicpm-v
  ollama pull llama3.2:3b
  ollama pull mistral:7b
  ollama pull deepseek-r1:1.5b
  ollama pull mxbai-embed-large
  ```
- To use the RAG example, you'll also need the docker compose and do a "RAG prepare"
- Then run the WebUI project

## Individual examples

See "Additional prerequisites" as described in the "Running with WebUI" section.
All examples can also be run as a standalone application.

## References

Presentation: https://sk.netwatwezoeken.nl/

`issues.csv` in the RAG example taken from: https://huggingface.co/datasets/giseldo/neodataset

[Prompty.ai](https://prompty.ai/)