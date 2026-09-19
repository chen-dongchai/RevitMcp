using Autodesk.Revit.UI;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revits.Actions
{
    public interface IRevitAction
    {
        TcpResponse Execute(UIApplication app, TcpRequest request);
    }
}
