using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IAuditRepository
    {
        void AddAuditEvent(AuditEvent entity);
        Task<DataListResponse<TResult>> PageAuditsAsync<TResult>(Expression<Func<AuditEvent, bool>> filter, Expression<Func<AuditEvent, TResult>> select, Pagination pagination, CancellationToken ct = default);
    }
}
