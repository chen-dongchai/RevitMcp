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

            OpenAIClient openAIClient = new OpenAIClient(             //针对使用的ai不同，这一部分也可能不同
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions() { Endpoint = new Uri(endpoint) }
            );

            // ---------- 1.5 预检 API Key ----------
            Console.Write("正在验证 API Key... ");
            try
            {
               

                var probe = new List<ChatMessage>
    {
        new(ChatRole.User, "hi")
    };
                var probeOptions = new ChatOptions { MaxOutputTokens = 1 };

                // 获取 IChatClient 实例
                var probeClient = openAIClient.GetChatClient(modelId).AsIChatClient();

                await foreach (var _ in probeClient.GetStreamingResponseAsync(probe, probeOptions))
                {
                                                     // 只要收到任何响应，就说明 Key 有效
                    break;
                }

                Console.WriteLine("OK");
            }
            catch (System.ClientModel.ClientResultException ex) when (ex.Status == 401)
            {
                Console.WriteLine("失败\n");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("  API Key 认证失败 (HTTP 401)");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine($"  当前 Key 末尾: ****{apiKey.Substring(Math.Max(0, apiKey.Length - 4))}");
                Console.WriteLine();
                Console.WriteLine("  排查步骤:");
                Console.WriteLine("  1. 去 https://platform.deepseek.com/ 检查 Key 状态");
                Console.WriteLine("  2. 确认 Key 未过期、未禁用、余额充足");
                Console.WriteLine("  3. 生成新 Key 后执行: setx apiKey \"sk-新Key\"");
                Console.WriteLine("  4. 完全关闭 Visual Studio 和终端，重新打开");
                Console.WriteLine();
                Console.WriteLine("  验证环境变量: 新开 cmd 执行 echo %apiKey%");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"失败\n");
                Console.WriteLine($"预检异常: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine("程序将退出。");
                Console.ReadKey();
                return;
            }




            // 构建支持函数调用的 IChatClient
            IChatClient client = new ChatClientBuilder(             
                    openAIClient.GetChatClient(modelId).AsIChatClient()          //针对使用的ai不同，此行内容也会不同
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
                    .GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))  //在这一行，ai模型不仅会思考需要使用哪些工具，还会调用这些工具，并把结果整合到最终的回复中。GetStreamingResponseAsync 方法会返回一个异步流，逐步生成模型的回复。
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
