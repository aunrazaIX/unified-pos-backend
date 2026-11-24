using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IOutboxRepository
    {
        void AddOutboxMessage(OutboxMessage entity);
    }
}
