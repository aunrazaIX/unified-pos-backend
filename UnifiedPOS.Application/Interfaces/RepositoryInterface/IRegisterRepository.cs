using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IRegisterRepository
    {
        Task<bool> AnyShiftsAsync(Expression<Func<Shift, bool>> filter, CancellationToken ct = default);
        void AddShift(Shift entity);
        Task<Shift?> FindShiftAsync(Expression<Func<Shift, bool>> filter, CancellationToken ct = default, bool tracked = true);
    }
}
