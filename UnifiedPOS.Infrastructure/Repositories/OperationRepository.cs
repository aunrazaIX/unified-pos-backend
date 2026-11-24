using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class OperationRepository(POSContext context) : IOperationRepository
    {
        private IQueryable<Operation> OperationsQuery(bool tracked)
        {
            IQueryable<Operation> query = context.Set<Operation>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<Operation?> FindOperationAsync(Expression<Func<Operation, bool>> filter, CancellationToken ct = default, bool tracked = true) => OperationsQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public void AddOperation(Operation entity) => context.Set<Operation>().Add(entity);
    }
}
