using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Protocols
{
    public static class TcpProtocol
    {
        // ============ 异步版本（MCP Server 用） ============

        public static async Task WriteMessageAsync<T>(
            Stream stream, T message, CancellationToken ct = default)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(message, JsonConfig.Options);

            byte[] header = new byte[ProtocolConstants.HeaderSize];
            BinaryPrimitives.WriteInt32LittleEndian(header, json.Length);

            await stream.WriteAsync(header, 0, header.Length, ct);
            await stream.WriteAsync(json, 0, json.Length, ct);
            await stream.FlushAsync(ct);
        }

        public static async Task<T> ReadMessageAsync<T>(
            Stream stream, CancellationToken ct = default)
        {
            byte[] header = new byte[ProtocolConstants.HeaderSize];
            await ReadExactAsync(stream, header, 0, header.Length, ct);

            int length = BinaryPrimitives.ReadInt32LittleEndian(header);
            if (length <= 0 || length > ProtocolConstants.MaxMessageSize)
                throw new InvalidDataException($"非法消息长度: {length}");

            byte[] payload = new byte[length];
            await ReadExactAsync(stream, payload, 0, length, ct);

            return JsonSerializer.Deserialize<T>(payload, JsonConfig.Options)!;
        }

        private static async Task ReadExactAsync(
            Stream stream, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            while (count > 0)
            {
                int read = await stream.ReadAsync(buffer, offset, count, ct);
                if (read == 0) throw new EndOfStreamException("连接已关闭");
                offset += read;
                count -= read;
            }
        }

        // ============ 同步版本（Revit 阻塞侧用） ============

        public static void WriteMessage<T>(Stream stream, T message)
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(message, JsonConfig.Options);

            byte[] header = new byte[ProtocolConstants.HeaderSize];
            BinaryPrimitives.WriteInt32LittleEndian(header, json.Length);

            stream.Write(header, 0, header.Length);
            stream.Write(json, 0, json.Length);
            stream.Flush();
        }

        public static T ReadMessage<T>(Stream stream)
        {
            byte[] header = new byte[ProtocolConstants.HeaderSize];
            ReadExact(stream, header, 0, header.Length);

            int length = BinaryPrimitives.ReadInt32LittleEndian(header);
            if (length <= 0 || length > ProtocolConstants.MaxMessageSize)
                throw new InvalidDataException($"非法消息长度: {length}");

            byte[] payload = new byte[length];
            ReadExact(stream, payload, 0, length);

            return JsonSerializer.Deserialize<T>(payload, JsonConfig.Options)!;
        }

        private static void ReadExact(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read == 0) throw new EndOfStreamException("连接已关闭");
                offset += read;
                count -= read;
            }
        }
    }
}
