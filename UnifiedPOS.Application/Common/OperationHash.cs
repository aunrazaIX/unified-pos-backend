using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UnifiedPOS.Application.Common
{
    public static class OperationHash
    {
        public static string Of<T>(T value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    }
}
