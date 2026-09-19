using Autodesk.Revit.UI;
using Revits.Infrastructure;
using Revits.Schduling;
using Revits.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revits
{
    public class RevitApp : IExternalApplication
    {
        // 生命周期与 App 一致，必须保存为字段
        private McpExternalEventHandler _handler;
        private ExternalEvent _externalEvent;
        private TcpTransportServer _server;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Logger.Info("MCP 插件启动中...");

                var dispatcher = new RequestDispatcher();
                _handler = new McpExternalEventHandler(dispatcher);
                _externalEvent = ExternalEvent.Create(_handler);
                _server = new TcpTransportServer(_handler, _externalEvent);
                _server.Start();

                Logger.Info("MCP 插件启动成功, 端口=" + _server.Port);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.Error("MCP 插件启动失败", ex);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                Logger.Info("MCP 插件关闭中...");

                // 停止 TCP 服务端（内部停线程 + 删发现文件）
                if (_server != null)
                    _server.Stop();

                Logger.Info("MCP 插件已关闭");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.Error("MCP 插件关闭异常", ex);
                return Result.Failed;
            }
        }
    }
}
