using Autodesk.Revit.UI;
using Shared.Discovery;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared.Protocols;
using Revits.Schduling;
using Revits.Infrastructure;

namespace Revits.Transport
{
    public class TcpTransportServer
    {
        private readonly McpExternalEventHandler _handler;
        private readonly ExternalEvent _externalEvent;
        private readonly string _authToken = Guid.NewGuid().ToString("N");

        private TcpListener _listener;
        private volatile bool _running;
        public int Port { get; private set; }

        public TcpTransportServer(McpExternalEventHandler handler, ExternalEvent externalEvent)
        {
            _handler = handler;
            _externalEvent = externalEvent;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            DiscoveryFile.Write(new DiscoveryInfo
            {
                Port = Port,
                AuthToken = _authToken,
                Pid = Process.GetCurrentProcess().Id
            });

            _running = true;
            new Thread(AcceptLoop) { IsBackground = true }.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener?.Stop();
            DiscoveryFile.Delete();
        }

        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    new Thread(() => HandleClient(client)) { IsBackground = true }.Start();
                }
                catch { break; }
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                try
                {
                    var auth = TcpProtocol.ReadMessage<AuthMessage>(stream);
                    if (auth.AuthToken != _authToken) return;

                    while (_running)
                    {
                        var request = TcpProtocol.ReadMessage<TcpRequest>(stream);

                        var tcs = new TaskCompletionSource<TcpResponse>();
                        _handler.Enqueue(request, tcs);
                        _externalEvent.Raise();     //主线程处理任务

                        if (!tcs.Task.Wait(Timeouts.RequestMs))
                        {
                            TcpProtocol.WriteMessage(stream, new TcpResponse
                            {
                                RequestId = request.RequestId,
                                Success = false,
                                Message = "超时"
                            });
                            continue;
                        }

                        TcpProtocol.WriteMessage(stream, tcs.Task.Result);
                    }
                }
                catch { }
            }
        }

        private class AuthMessage { public string AuthToken { get; set; } }
    }
}
