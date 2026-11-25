using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Bootstrap;

public sealed class OwnerProvisioner(POSContext context, IPasswordHasher<User> hasher, ILogger<OwnerProvisioner> logger)
{
    public async Task ProvisionAsync(BootstrapOptions options, bool isDevelopment, CancellationToken ct = default)
    {
        if (!options.Enabled) { logger.LogInformation("Owner provisioning skipped."); return; }
        if (!options.IsValid() || (!isDevelopment && !options.AllowProduction))
            throw new InvalidOperationException("Bootstrap configuration is invalid or provisioning is not enabled for this environment.");

        await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            context.ChangeTracker.Clear();
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            // Serialize concurrent application starts, including when the organization table is empty.
            await context.Database.ExecuteSqlRawAsync("DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource=N'UnifiedPOS.InitialOwner', @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=30000; IF @result < 0 THROW 51000, 'Bootstrap lock could not be acquired.', 1;", ct);
            var organizations = await context.Set<Organization>().Take(2).ToListAsync(ct);
            if (organizations.Count > 1 || (organizations.Count == 1 && organizations[0].Name != options.OrganizationName.Trim()))
                throw new InvalidOperationException("Bootstrap organization does not match the single-organization database.");
            var organization = organizations.SingleOrDefault();
            if (organization == null)
            {
                organization = new Organization { Name = options.OrganizationName.Trim(), Currency = options.Currency };
                context.Add(organization);
            }
            var normalized = options.OwnerUserName.Trim().ToUpperInvariant();
            var owner = await context.Set<User>().SingleOrDefaultAsync(x => x.OrganizationId == organization.Id && x.NormalizedUserName == normalized, ct);
            if (owner != null)
            {
                if (owner.Role != UserRole.Owner || !owner.IsActive)
                    throw new InvalidOperationException("Bootstrap account already exists without an active Owner role.");
                logger.LogInformation("Owner provisioning skipped.");
            }
            else
            {
                if (await context.Set<User>().AnyAsync(x => x.OrganizationId == organization.Id && x.Role == UserRole.Owner, ct))
                    throw new InvalidOperationException("An Owner already exists. Bootstrap cannot create another initial Owner.");
                owner = new User { OrganizationId = organization.Id, UserName = options.OwnerUserName.Trim(), NormalizedUserName = normalized, Role = UserRole.Owner };
                owner.PasswordHash = hasher.HashPassword(owner, options.OwnerPassword);
                context.Add(owner);
                await context.SaveChangesAsync(ct);
            }
            await transaction.CommitAsync(ct);
            if (owner != null) logger.LogInformation("Owner provisioning completed.");
        });
    }
}
