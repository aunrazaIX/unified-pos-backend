using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class UserRepository(POSContext context) : IUserRepository
    {
        private IQueryable<User> UsersQuery(bool tracked)
        {
            IQueryable<User> query = context.Set<User>().Include(x => x.Locations).Include(x => x.Registers).AsSplitQuery();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<User?> FindUserAsync(Expression<Func<User, bool>> filter, CancellationToken ct = default, bool tracked = true) => UsersQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public Task<DataListResponse<TResult>> PageUsersAsync<TResult>(Expression<Func<User, bool>> filter, Expression<Func<User, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<User>(), filter, select, pagination, ct);

        public Task<bool> AnyUsersAsync(Expression<Func<User, bool>> filter, CancellationToken ct = default) => context.Set<User>().AnyAsync(filter, ct);

        public void AddUser(User entity) => context.Set<User>().Add(entity);

        private IQueryable<Session> SessionsQuery(bool tracked)
        {
            IQueryable<Session> query = context.Set<Session>();
            return tracked ? query : query.AsNoTracking();
        }

        public void AddSession(Session entity) => context.Set<Session>().Add(entity);

        public Task<Session?> FindSessionAsync(Expression<Func<Session, bool>> filter, CancellationToken ct = default, bool tracked = true) => SessionsQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public Task<List<Session>> ListSessionsAsync(Expression<Func<Session, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => SessionsQuery(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);
    }
}
