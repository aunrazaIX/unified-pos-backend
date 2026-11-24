
namespace UnifiedPOS.Application.Common
{
    public class PosException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public static PosException Invalid(string message) => new(400, message);
        public static PosException Missing() => new(404, "Record not found.");
        public static PosException Forbidden() => new(403, "Access denied.");
        public static PosException Conflict(string message) => new(409, message);
    }
}
