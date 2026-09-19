using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class TcpResponse
    {
        public string RequestId { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
    }
}
