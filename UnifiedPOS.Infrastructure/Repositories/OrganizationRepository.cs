using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class OrganizationRepository(POSContext context) : IOrganizationRepository
    {
        private IQueryable<Organization> OrganizationsQuery(bool tracked)
        {
            IQueryable<Organization> query = context.Set<Organization>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<Organization?> FindOrganizationAsync(Expression<Func<Organization, bool>> filter, CancellationToken ct = default, bool tracked = true) => OrganizationsQuery(tracked).SingleOrDefaultAsync(filter, ct);

        private IQueryable<Location> LocationsQuery(bool tracked)
        {
            IQueryable<Location> query = context.Set<Location>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<Location?> FindLocationAsync(Expression<Func<Location, bool>> filter, CancellationToken ct = default, bool tracked = true) => LocationsQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public Task<DataListResponse<TResult>> PageLocationsAsync<TResult>(Expression<Func<Location, bool>> filter, Expression<Func<Location, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Location>(), filter, select, pagination, ct);

        public void AddLocation(Location entity) => context.Set<Location>().Add(entity);

        public Task<List<Location>> ListLocationsAsync(Expression<Func<Location, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => LocationsQuery(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);

        private IQueryable<Register> RegistersQuery(bool tracked)
        {
            IQueryable<Register> query = context.Set<Register>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<Register?> FindRegisterAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default, bool tracked = true) => RegistersQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public Task<List<Register>> ListRegistersAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => RegistersQuery(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);

        public Task<DataListResponse<TResult>> PageRegistersAsync<TResult>(Expression<Func<Register, bool>> filter, Expression<Func<Register, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Register>(), filter, select, pagination, ct);

        public void AddRegister(Register entity) => context.Set<Register>().Add(entity);

        public Task<bool> AnyRegistersAsync(Expression<Func<Register, bool>> filter, CancellationToken ct = default) => context.Set<Register>().AnyAsync(filter, ct);
    }
}
