using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Protocols
{
    public static class ProtocolConstants
    {
        public const string DiscoveryDirName = "RevitMcp";
        public const string DiscoveryFileFormat = "revit-{0}.json";

        public const int MaxMessageSize = 10_000_000;
        public const int HeaderSize = 4;
      
        public const string ActionEcho = "echo";
        public const string ActionCreateWall = "create_wall";
        public const string ActionQueryElements = "query_elements";
        public const string ActionDeleteElement = "delete_element";
    }
}
//定义一些通信两方都需要用到和同一的固定值，比如消息头长度，最大消息长度，发现文件路径等。还有各种方法发对应名。