using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revits.Infrastructure
{
    public static class Timeouts
    {
        /// <summary>
        /// 单次请求等待 Revit 主线程的最大毫秒数。
        /// 超过这个时间没返回，视为超时。
        /// </summary>
        public const int RequestMs = 30_000;

        /// <summary>
        /// token 握手超时（可选，如果流设置了 ReadTimeout）。
        /// </summary>
        public const int AuthMs = 3_000;
    }
}
