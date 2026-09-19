using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class DiscoveryInfo
    {
        public int SchemaVersion { get; set; } = 2;//协议版本号
        public int RevitYear { get; set; } = 2022;
        public string Transport { get; set; } = "tcp";
        public int Port { get; set; }//端口
        public string AuthToken { get; set; } = "";//认证token
        public int Pid { get; set; }//进程id
    }
}
