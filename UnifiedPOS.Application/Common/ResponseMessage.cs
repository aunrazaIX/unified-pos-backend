
namespace UnifiedPOS.Application.Common
{
    public class ResponseMessage
    {
        public bool success { get; init; }
        public object? data { get; init; }
        public string message { get; init; } = "";
        public static ResponseMessage Ok(object? data = null, string message = "") => new() { success = true, data = data, message = message };
    }
}
