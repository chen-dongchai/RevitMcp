using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;

namespace Revits.Actions
{
    public class EchoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "Revit-echo: " + request.Action,
                Data = new
                {
                    action = request.Action,
                    parameters = request.Parameters,
                    timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
}
