using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.Common
{
    public class ResponseMessage
    {
        public bool success { get; init; }
        public object? data { get; init; }
        public string message { get; init; } = "";
        public static ResponseMessage Ok(object? data = null, string message = "") => new() { success = true, data = data, message = message };
    }

    public class Pagination
    {
        [Range(1, 1000000)] public int CurrentPage { get; set; } = 1;
        [Range(1, 100)] public int PageSize { get; set; } = 25;
        [StringLength(100)] public string? SearchString { get; set; }
    }
    public record DataListResponse<T>(IReadOnlyList<T> Items, int TotalCount, int CurrentPage, int PageSize);
    public class PosException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public static PosException Invalid(string message) => new(400, message);
        public static PosException Missing() => new(404, "Record not found.");
        public static PosException Forbidden() => new(403, "Access denied.");
        public static PosException Conflict(string message) => new(409, message);
    }
    public static class Roles
    {
        public const string Administrators = "Owner,Admin";
        public const string Managers = "Owner,Admin,Manager";
    }
    public static class Money
    {
        public static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        public static void Validate(decimal value, bool allowZero = true)
        {
            if (value < 0 || (!allowZero && value == 0) || value > 100000000m || Round(value) != value)
                throw PosException.Invalid("Amount must be within range and have at most two decimal places.");
        }
    }
    public static class OperationHash
    {
        public static string Of<T>(T value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    }
    public class JwtSettings
    {
        [Required, MinLength(32)] public string Key { get; set; } = "";
        [Required] public string Issuer { get; set; } = "";
        [Required] public string Audience { get; set; } = "";
        [Range(1, 30)] public int AccessMinutes { get; set; } = 15;
        [Range(1, 24)] public int SessionHours { get; set; } = 8;
    }
    public class PosSettings
    {
        public bool AllowNegativeStock { get; set; }
        public bool OfflineReplayEnabled { get; set; }
    }
}
