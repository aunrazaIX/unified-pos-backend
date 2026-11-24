using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class AuditRepository(POSContext context) : IAuditRepository
    {
        public void AddAuditEvent(AuditEvent entity) => context.Set<AuditEvent>().Add(entity);

        public Task<DataListResponse<TResult>> PageAuditsAsync<TResult>(Expression<Func<AuditEvent, bool>> filter, Expression<Func<AuditEvent, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<AuditEvent>(), filter, select, pagination, ct);
    }
}
