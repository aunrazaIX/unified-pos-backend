using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class OutboxRepository(POSContext context) : IOutboxRepository
    {
        public void AddOutboxMessage(OutboxMessage entity) => context.Set<OutboxMessage>().Add(entity);
    }
}
