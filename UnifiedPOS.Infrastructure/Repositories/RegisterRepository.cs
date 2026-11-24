using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class RegisterRepository(POSContext context) : IRegisterRepository
    {
        private IQueryable<Shift> ShiftsQuery(bool tracked)
        {
            IQueryable<Shift> query = context.Set<Shift>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<bool> AnyShiftsAsync(Expression<Func<Shift, bool>> filter, CancellationToken ct = default) => context.Set<Shift>().AnyAsync(filter, ct);

        public void AddShift(Shift entity) => context.Set<Shift>().Add(entity);

        public Task<Shift?> FindShiftAsync(Expression<Func<Shift, bool>> filter, CancellationToken ct = default, bool tracked = true) => ShiftsQuery(tracked).SingleOrDefaultAsync(filter, ct);
    }
}
