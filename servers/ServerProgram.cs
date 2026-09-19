using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Servers;
using System.ComponentModel;

namespace servers
{
    internal class ServerProgram
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);// 创建应用程序构建器

            // 日志写 stderr，不能污染 stdout（stdout 被 MCP 协议占用）
            builder.Logging.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Trace;
            });

            // 注册 RevitTcpClient 为单例（长连接，整个进程共享一条 TCP）
            builder.Services.AddSingleton<RevitTcpClient>();

            builder.Services//配置依赖并注入容器
                .AddMcpServer()// 注册 mcp 服务器核心服务
                .WithStdioServerTransport() // 配置传输方式为标准输入/输出
                .WithToolsFromAssembly();  // 自动发现并注册工具

            // 启动应用程序并保持运行状态，等待处理新的任务
            await builder.Build().RunAsync(); //保证ai工具能一直保持在运行状态，时刻准备处理新的任务，只有接收到退出命令时才会停止运行
                                              //同时这也是一种优雅退出，使用await等待完成后再退出，避免了强制终止可能导致的资源泄漏或数据丢失问题，最后返回task的结果，确保程序的退出状态能够正确反映运行过程中的情况。
        }
    }
}
