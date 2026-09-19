# Revit MCP

> **自主开发的 Revit 2022 MCP 模板**
>
> 本项目是一个可复用的 Revit 2022 + MCP（Model Context Protocol）集成模板，用于把 AI 能力接入 Revit。可作为二次开发的起点，按需扩展自己的 Revit 工具。

让 AI 控制 Autodesk Revit 2022 的 MCP 服务器。

## 架构

```
AI 客户端 (TextHost/Claude)  ──stdio──▶  MCP Server (.NET 8)
                                              │
                                              │ TCP 127.0.0.1
                                              ▼
                                        Revit 插件 (.NET Framework 4.8)
                                              │
                                              │ ExternalEvent
                                              ▼
                                        Revit API 主线程
```

## 项目结构

| 项目 | 目标框架 | 作用 |
|---|---|---|
| `Revits` | net48 | Revit 插件：TCP 监听、ExternalEvent 调度 |
| `Servers` | net8.0 | MCP Server：处理 MCP 协议、转发请求 |
| `Shared` | net48;net8.0 | 协议定义：消息模型、长度前缀、发现文件 |
| `TextHost` | net8.0 | 示例 MCP 客户端（接入 DeepSeek） |

## 通信流程

1. Revit 启动 → 插件绑定 `127.0.0.1:0`（OS 分配端口）→ 写发现文件 `%LOCALAPPDATA%\RevitMcp\revit-2022.json`（含端口、token、PID）
2. MCP Server 读发现文件 → 连接 → 发 token → 建立长连接
3. AI 调用 tool → MCP Server 发 `TcpRequest` → Revit 后台线程收 → `Enqueue` + `ExternalEvent.Raise()` → 主线程执行 Revit API → 结果原路返回

## 快速开始

### 1. 部署 Revit 插件

编译 `Revits`，把 `bin\Debug\` 下所有 `.dll` 复制到：
```
%APPDATA%\Autodesk\Revit\Addins\2022\RevitTextMcp\
```

`.addin` 文件放到：
```
%APPDATA%\Autodesk\Revit\Addins\2022\texthostrevit.addin
```

内容：
```xml
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Revits</Name>
    <Assembly>RevitTextMcp\Revits.dll</Assembly>
    <ClientId>你的唯一GUID</ClientId>
    <FullClassName>Revits.RevitApp</FullClassName>
    <VendorId>NAME</VendorId>
    <VendorDescription>Your Company</VendorDescription>
  </AddIn>
</RevitAddIns>
```

首次启动 Revit 会弹安全警告，点"总是载入"。

### 2. 配置 MCP Client

`TextHost` 里的 `StdioClientTransport` 指向 `Servers` 的 DLL：
```csharp
Arguments = [@"你的路径\Servers\bin\Debug\net8.0\Servers.dll"]
```

设置环境变量 `apiKey`（DeepSeek API Key）。

### 3. 运行

1. 启动 Revit（确认插件加载）
2. 运行 `TextHost`
3. 输入 `测试 echo`

## 扩展 Tool

新增一个 tool 需要改四处：

**1. Shared/ProtocolConstants.cs**
```csharp
public const string ActionMyTool = "my_tool";
```

**2. Revits/Actions/MyToolAction.cs**
```csharp
public class MyToolAction : IRevitAction
{
    public TcpResponse Execute(UIApplication app, TcpRequest request)
    {
        var doc = app.ActiveUIDocument.Document;
        using (var tx = new Transaction(doc, "MyTool"))
        {
            tx.Start();
            // 调用 Revit API
            tx.Commit();
        }
        return new TcpResponse { RequestId = request.RequestId, Success = true };
    }
}
```

**3. Revits/Scheduling/RequestDispatcher.cs**
```csharp
Register(ProtocolConstants.ActionMyTool, new MyToolAction());
```

**4. Servers/McpTools.cs**
```csharp
[McpServerTool]
[Description("...")]
public async Task<string> MyTool(...) 
    => await _client.SendAsync(new TcpRequest { Action = ProtocolConstants.ActionMyTool, ... });
```

## 技术要点

- **ExternalEvent**：Revit API 只能在主线程调用，后台线程通过 `ExternalEvent.Raise()` 投递任务
- **长度前缀协议**：`[4字节长度][JSON]` 解决 TCP 粘包
- **发现文件**：解决动态端口问题，MCP Server 通过 `%LOCALAPPDATA%\RevitMcp\revit-2022.json` 找到 Revit
- **Token 认证**：首次连接发送 `Guid` 生成的 token，防止本机其他进程访问
- **静态类安全**：`Logger` 等静态辅助类的静态构造函数必须包 `try/catch`，避免 `TypeInitializationException` 拖垮插件

## 日志

- Revit 侧：`%LOCALAPPDATA%\RevitMcp\logs\revit-*.log`
- Server 侧：`C:\temp\mcp-client.log`（可自行调整）

## 环境要求

- Revit 2022
- .NET Framework 4.8（Revit 插件）
- .NET 8.0 SDK（Server 和 Client）
- Visual Studio 2022

## 注意事项

这是一个**模板项目**，用于演示 Revit 2022 与 MCP 的集成方式。实际使用时请根据需求：

- 替换为你的 AI 客户端（当前 TextHost 接入 DeepSeek 仅作示例）
- 添加你需要的 Revit 工具（当前只有 `echo` 测试工具）
- 补充错误处理、日志、并发控制等生产级功能
- 注意 Revit 插件的安全警告（未签名 DLL 每次启动会提示）

## License

MIT
