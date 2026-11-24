
namespace UnifiedPOS.Application.Common
{
    public record DataListResponse<T>(IReadOnlyList<T> Items, int TotalCount, int CurrentPage, int PageSize);
}
