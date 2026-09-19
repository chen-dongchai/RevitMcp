using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Protocols;

namespace Servers
{
    [McpServerToolType]
    public class EchoTool
    {
        private readonly RevitTcpClient _client;

        public EchoTool(RevitTcpClient client)
        {
            _client = client;
        }

        [McpServerTool]
        [Description("将用户输入发送到 Revit 插件并原样返回。仅用于测试 MCP Server 与 Revit 的连接是否通畅。")]
        public async Task<string> Echo(string message)
        {

            var request = new TcpRequest
            {
                Action = ProtocolConstants.ActionEcho,
                Parameters = new Dictionary<string, string>
                {
                    ["message"] = message
                }
            };

            TcpResponse response = await _client.SendAsync(request);

            if (!response.Success)
                return "失败: " + response.Message;

            return JsonSerializer.Serialize(response.Data, JsonConfig.Options);
        }
    }
}
