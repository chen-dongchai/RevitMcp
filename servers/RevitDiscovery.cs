using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Discovery;
using Shared.Models;
namespace Servers
{
    public static class RevitDiscovery
    {
        public static DiscoveryInfo Find()
        {
            DiscoveryInfo info = DiscoveryFile.Read();
            if (info == null) return null;

            if (!IsProcessAlive(info.Pid))
            {
                try { DiscoveryFile.Delete(); } catch { }
                return null;
            }

            return info;
        }

        private static bool IsProcessAlive(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                return !p.HasExited;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
