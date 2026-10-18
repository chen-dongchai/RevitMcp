using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Protocols;
using Revits.Actions;
using Revits.Infrastructure;

namespace Revits.Schduling
{
    public class RequestDispatcher
    {
        // 路由表：Action 名称 → 具体操作
        private readonly Dictionary<string, IRevitAction> _actions
            = new Dictionary<string, IRevitAction>();

        public RequestDispatcher()
        {
            // 注册所有支持的操作
            
            Register(ProtocolConstants.ActionEcho, new EchoAction());
            Register(ProtocolConstants.ActionGetCategoryInfo, new GetCategoryInfoAction());

            Register(ProtocolConstants.ActionGetProjectInfo, new GetProjectInfoAction());
            Register(ProtocolConstants.ActionGetModelStatistics, new GetModelStatistics());
            Register(ProtocolConstants.ActionGetLevelInfo, new GetLevelInfoAction());
            Register(ProtocolConstants.ActionGetViewInfo, new GetViewInfoAction());
            Register(ProtocolConstants.ActionGetViewSheetsInfo, new GetViewSheetsInfoAction());
            Register(ProtocolConstants.ActionGetProjectFamilyTypes, new GetProjectFamilyTypesAction());
            Register(ProtocolConstants.ActionGetFamilyTypeParameters, new GetFamilyTypeParametersAction());
            Register(ProtocolConstants.ActionGetElementInstanceParameters, new GetElementInstanceParametersAction());
            Register(ProtocolConstants.ActionGetElementIdsByCategoryId, new GetElementIdsByCategoryIdAction());
            Register(ProtocolConstants.ActionIdentifyUnknownElementId, new IdentifyUnknownElementIDAction());
            Register(ProtocolConstants.ActionGetElementLocation, new GetElementLocationAction());
            Register(ProtocolConstants.ActionGetElementGeometry, new GetElementGeometryAction());
            Register(ProtocolConstants.ActionGetHostOpeningsAndCuts, new GetHostOpeningsAndCutsAction());
            Register(ProtocolConstants.ActionGetSpatialRelation, new GetSpatialRelationAction());

            // 新增 tool 在这里加一行
        }

        public void Register(string name, IRevitAction action)
        {
            _actions[name] = action;
        }

        /// <summary>
        /// 被 McpExternalEventHandler.Execute 调用，运行在 Revit 主线程。
        /// </summary>
        public TcpResponse Dispatch(UIApplication app, TcpRequest request)
        {
            if (!_actions.TryGetValue(request.Action, out IRevitAction action))
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "未知操作: " + request.Action
                };
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            Logger.Info("执行 Action: " + request.Action);

            TcpResponse response = action.Execute(app, request);

            Logger.Info(request.Action + " 完成, 耗时 " + sw.ElapsedMilliseconds + "ms");
            return response;
        }
    }
}
