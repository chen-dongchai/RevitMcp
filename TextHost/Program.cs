using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;

namespace RevitMcpHost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // ---------- 1. 配置 DeepSeek 客户端 ----------
            string modelId = "deepseek-chat";
            string endpoint = "https://api.deepseek.com/v1";
            string apiKey = Environment.GetEnvironmentVariable("apiKey");

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("错误：未设置环境变量 apiKey");
                return;
            }

            OpenAIClient openAIClient = new OpenAIClient(          //针对使用的ai不同，这一部分也可能不同
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions() { Endpoint = new Uri(endpoint) }
            );

            // 构建支持函数调用的 IChatClient
            IChatClient client = new ChatClientBuilder(             
                    openAIClient.GetChatClient(modelId).AsIChatClient()    //针对使用的ai不同，此处内容也会不同
                )
                .UseFunctionInvocation()   // 启用工具调用
                .Build();

            // ---------- 2. 创建 MCP 客户端 ----------
            var transport = new StdioClientTransport(new()
            {
                Command = "dotnet",
                Arguments = [@"C:\Users\33689\Desktop\Csharp\servers\bin\Debug\net8.0\Servers.dll"], // 替换为实际路径
                Name = "Minimal MCP Server",
            });

            await using McpClient mcpClient = await McpClient.CreateAsync(transport);

            // ---------- 3. 获取并显示工具 ----------
            Console.WriteLine("可用工具:");
            IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
            foreach (McpClientTool tool in tools)
            {
                Console.WriteLine($"- {tool.Name}: {tool.Description}");
            }
            Console.WriteLine();

            // ---------- 4. 对话循环 ----------
            List<ChatMessage> messages = [             //这个大概就是上下文或者说对话历史
                new ChatMessage(ChatRole.System, "你是DeepSeek，由深度求索公司开发的 AI 助手。请始终诚实、准确地回答用户的问题。")];
            while (true)
            {
                Console.Write("Prompt: ");
                string? userInput = Console.ReadLine();       //获取用户输入的内容
                if (string.IsNullOrEmpty(userInput)) break;

                messages.Add(new(ChatRole.User, userInput));  //把用户输入加入 messages，这样模型在生成回复时就能看到用户说了什么。

                List<ChatResponseUpdate> updates = [];//[] 是 C# 12 的集合表达式，等价于 new List<ChatResponseUpdate>()。
                await foreach (ChatResponseUpdate update in client
                    .GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))  //用于流式获取 AI 回复。
                {
                    Console.Write(update.Text);  //把 AI 回复的文本输出到控制台
                    updates.Add(update);//把这个更新块收集起来，等全部结束后再统一合并成完整的回复消息
                }
                Console.WriteLine(); //输出一个换行，让下一轮提示从新行开始。
                messages.AddMessages(updates);  //把收集到的所有 ChatResponseUpdate 合并成一条或多条 ChatMessage（角色通常是 Assistant），并添加到 messages 历史中。
            }
        }
    }
}
