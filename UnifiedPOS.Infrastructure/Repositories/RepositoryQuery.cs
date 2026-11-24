using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Infrastructure.Repositories
{
    internal static class RepositoryQuery
    {
        internal static async Task<DataListResponse<TResult>> PageAsync<TEntity, TResult>(IQueryable<TEntity> source, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TResult>> select, Pagination pagination, CancellationToken ct) where TEntity : Entity
        {
            var query = source.AsNoTracking().Where(filter);
            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.CreatedOn).ThenBy(x => x.Id)
                .Skip((pagination.CurrentPage - 1) * pagination.PageSize).Take(pagination.PageSize).Select(select).ToListAsync(ct);
            return new(items, count, pagination.CurrentPage, pagination.PageSize);
        }
    }
}
