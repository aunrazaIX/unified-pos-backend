using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IOrganizationRepository
    {
        Task<Organization?> FindOrganizationAsync(Expression<Func<Organization, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<Location?> FindLocationAsync(Expression<Func<Location, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<DataListResponse<TResult>> PageLocationsAsync<TResult>(Expression<Func<Location, bool>> filter, Expression<Func<Location, TResult>> select, Pagination pagination, CancellationToken ct = default);
        void AddLocation(Location entity);
        Task<List<Location>> ListLocationsAsync(Expression<Func<Location, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
        Task<Register?> FindRegisterAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<List<Register>> ListRegistersAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
        Task<DataListResponse<TResult>> PageRegistersAsync<TResult>(Expression<Func<Register, bool>> filter, Expression<Func<Register, TResult>> select, Pagination pagination, CancellationToken ct = default);
        void AddRegister(Register entity);
        Task<bool> AnyRegistersAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default);
    }
}
