using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IUserRepository
    {
        Task<User?> FindUserAsync(Expression<Func<User, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<DataListResponse<TResult>> PageUsersAsync<TResult>(Expression<Func<User, bool>> filter, Expression<Func<User, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<bool> AnyUsersAsync(Expression<Func<User, bool>> filter, CancellationToken ct = default);
        void AddUser(User entity);
        void AddSession(Session entity);
        Task<Session?> FindSessionAsync(Expression<Func<Session, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<List<Session>> ListSessionsAsync(Expression<Func<Session, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
    }
}
