using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IOperationRepository
    {
        Task<Operation?> FindOperationAsync(Expression<Func<Operation, bool>> filter, CancellationToken ct = default, bool tracked = true);
        void AddOperation(Operation entity);
    }
}
