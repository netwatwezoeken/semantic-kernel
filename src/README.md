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
    ollama pull llama3.2:3b
    ollama pull mistral:7b
    ollama pull deepseek-r1:1.5b
    ollama pull mxbai-embed-large
    ```
- Enough RAM for the GPU/CPU. Approx 8GB

The examples are intended to demonstrate and experiment with the Semantic Kernel library. And thus the models are kept as lightweight as possible.

## Run the examples

All examples can be run as a standalone application. Most examples are also available  via `WebUI`. To also see messages http content to and from the LLMs run the Aspire project `AppHost`.

### 01 Basic Chat
A fundamental chat implementation demonstrating the core chat functionality.

### 02 Chat History
Extends the basic chat with history to have a conversation

### 03 Templating
Shows how templates ([Prompty.ai](https://prompty.ai/)) can be used. 

### 04 Functions
Demonstrates how to use tools. A way have the LLM call your code.

### 06 Agent Framework
Implementation showcasing AI agent orchestration

### 07 RAG
Retrieval Augmented Generation (RAG)

## References

Presentation: https://sk.netwatwezoeken.nl/

`issues.csv` in the RAG example taken from: https://huggingface.co/datasets/giseldo/neodataset

[Prompty.ai](https://prompty.ai/)