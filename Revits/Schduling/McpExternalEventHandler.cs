using Autodesk.Revit.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;
using Revits.Infrastructure;

namespace Revits.Schduling
{
    public class McpExternalEventHandler : IExternalEventHandler
    {
        // 队列：后台线程入队，主线程出队
        private readonly ConcurrentQueue<PendingRequest> _queue
            = new ConcurrentQueue<PendingRequest>();

        // 路由表：根据 Action 名找到具体操作
        private readonly RequestDispatcher _dispatcher;

        public McpExternalEventHandler(RequestDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        // ==================== 后台线程调用 ====================

        /// <summary>
        /// 把请求放入队列，等待 Revit 主线程执行。
        /// 由 TcpTransportServer 的后台线程调用。
        /// </summary>
        public void Enqueue(TcpRequest request, TaskCompletionSource<TcpResponse> tcs)
        {
            _queue.Enqueue(new PendingRequest
            {
                Request = request,
                Tcs = tcs
            });
        }

        // ==================== Revit 主线程调用 ====================

        /// <summary>
        /// 由 Revit 主线程在消息循环空闲时调用。
        /// 不能是 async，里面不能 await，必须同步执行完毕。
        /// </summary>
        public void Execute(UIApplication app)
        {
            // 一次性处理完队列里所有请求
            while (_queue.TryDequeue(out PendingRequest pending))
            {
                TcpResponse response;

                try
                {
                    Logger.Info("Execute: " + pending.Request.Action);

                    // 交给 dispatcher 路由到具体 Action
                    response = _dispatcher.Dispatch(app, pending.Request);
                }
                catch (Exception ex)
                {
                    Logger.Error("Execute 异常: " + pending.Request.Action, ex);

                    response = new TcpResponse
                    {
                        RequestId = pending.Request.RequestId,
                        Success = false,
                        Message = ex.Message
                    };
                }

                // 把结果送回后台线程
                pending.Tcs.TrySetResult(response);
            }
        }

        public string GetName()
        {
            return "McpExternalEventHandler";
        }

        // ==================== 内部模型 ====================

        private class PendingRequest
        {
            public TcpRequest Request { get; set; }
            public TaskCompletionSource<TcpResponse> Tcs { get; set; }
        }
    }
}
