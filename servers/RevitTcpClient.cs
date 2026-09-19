using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Protocols;

namespace Servers
{
    public class RevitTcpClient : IAsyncDisposable
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private TcpClient _client;
        private NetworkStream _stream;

        public async Task<TcpResponse> SendAsync(
            TcpRequest request,
            CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);
            try
            {
                // 1. 确保已连接
                if (_client == null || !_client.Connected)
                    await ConnectInternalAsync(ct);

                // 2. 发请求
                await TcpProtocol.WriteMessageAsync(_stream, request, ct);

                // 3. 读响应
                TcpResponse response = await TcpProtocol.ReadMessageAsync<TcpResponse>(_stream, ct);
                return response;
            }
            catch (IOException)
            {
                ResetConnection();
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task ConnectInternalAsync(CancellationToken ct)
        {
            DiscoveryInfo info = RevitDiscovery.Find();
            if (info == null)
                throw new InvalidOperationException(
                    "未找到 Revit 插件。请确认 Revit 已启动且 MCP 插件已加载。");

            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", info.Port, ct);
            _stream = _client.GetStream();

            // 立即发送 token 认证
            var auth = new AuthMessage { AuthToken = info.AuthToken };
            await TcpProtocol.WriteMessageAsync(_stream, auth, ct);
        }

        private void ResetConnection()
        {
            try { _stream?.Dispose(); } catch { }
            try { _client?.Dispose(); } catch { }
            _stream = null;
            _client = null;
        }

        public async ValueTask DisposeAsync()
        {
            await _lock.WaitAsync();
            try { ResetConnection(); }
            finally { _lock.Release(); }
        }

        private class AuthMessage
        {
            public string AuthToken { get; set; }
        }
    }
}
