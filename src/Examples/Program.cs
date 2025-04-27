using Examples;
using Examples.BasicChat;
using Examples.ChatWithHistory;
using Examples.Functions;
using Examples.ImageRecognition;
using Examples.Prompty;

await Example<BasicChat>.Run();
await Example<ChatWithSystemPrompt>.Run();
await Example<ImageTemperatureReading>.Run();
await Example<PromptyTemplating>.Run();
await Example<FunctionExample>.Run();