using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TcpRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N");
        public string Action { get; set; } = "";
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
