using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.Common
{
    public class Pagination
    {
        [Range(1, 1000000)] public int CurrentPage { get; set; } = 1;
        [Range(1, 100)] public int PageSize { get; set; } = 25;
        [StringLength(100)] public string? SearchString { get; set; }
    }
}
