using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Protocols;
using Shared.Models;

namespace Shared.Discovery
{
    public static class DiscoveryFile
    {
        private static int revitYear = 2022;
        public static string GetDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProtocolConstants.DiscoveryDirName);
        }

        public static string GetPath(int revitYear)
        {
            string fileName = string.Format(ProtocolConstants.DiscoveryFileFormat, revitYear);
            return Path.Combine(GetDirectory(), fileName);
        }

        public static void Write(DiscoveryInfo info)
        {
            string path = GetPath(revitYear);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            string json = JsonSerializer.Serialize(info, JsonConfig.Options);
            File.WriteAllText(path, json);
        }

        public static DiscoveryInfo? Read()
        {
            string path = GetPath(revitYear);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<DiscoveryInfo>(json, JsonConfig.Options);
        }

        public static void Delete()
        {
            string path = GetPath(revitYear);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
