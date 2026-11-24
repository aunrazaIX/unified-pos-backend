using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Infrastructure.Databases
{
    public class POSContext(DbContextOptions<POSContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = typeof(Entity).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(Entity).IsAssignableFrom(t));
            foreach (var type in entityTypes)
            {
                var entity = modelBuilder.Entity(type);
                entity.HasKey(nameof(Entity.Id));
                entity.Property(nameof(Entity.Version)).IsRowVersion();
                foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(string)))
                    entity.Property(property.Name).HasMaxLength(1000);
                foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)))
                    entity.Property(property.Name).HasPrecision(19, 4);
            }
            modelBuilder.Entity<Category>().Property(x => x.Name).HasMaxLength(100);
            modelBuilder.Entity<Customer>().Property(x => x.Phone).HasMaxLength(20);
            modelBuilder.Entity<Location>().Property(x => x.Name).HasMaxLength(100);
            modelBuilder.Entity<Register>().Property(x => x.Name).HasMaxLength(100);
            modelBuilder.Entity<User>().Property(x => x.UserName).HasMaxLength(100);
            modelBuilder.Entity<User>().Property(x => x.NormalizedUserName).HasMaxLength(100);
            modelBuilder.Entity<OutboxMessage>().Property(x => x.Type).HasMaxLength(180);
            modelBuilder.Entity<Sale>().Property(x => x.ReceiptNumber).HasMaxLength(100);
            modelBuilder.Entity<Location>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<Location>().HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
            modelBuilder.Entity<Register>().HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId);
            modelBuilder.Entity<Register>().HasIndex(x => new { x.LocationId, x.Name }).IsUnique();
            modelBuilder.Entity<User>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<User>().HasIndex(x => new { x.OrganizationId, x.NormalizedUserName }).IsUnique();
            modelBuilder.Entity<User>().HasMany(x => x.Locations).WithOne().HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UserLocation>().HasKey(x => new { x.UserId, x.LocationId });
            modelBuilder.Entity<UserLocation>().HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId);
            modelBuilder.Entity<Session>().HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            modelBuilder.Entity<Session>().HasOne<Register>().WithMany().HasForeignKey(x => x.RegisterId);
            modelBuilder.Entity<Session>().Property(x => x.RefreshTokenHash).HasMaxLength(64);
            modelBuilder.Entity<Session>().HasIndex(x => x.RefreshTokenHash).IsUnique();
            modelBuilder.Entity<Category>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<Category>().HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();
            modelBuilder.Entity<Product>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<Product>().HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId);
            modelBuilder.Entity<Product>().HasOne<Product>().WithMany().HasForeignKey(x => x.ParentProductId);
            modelBuilder.Entity<Product>().HasOne<Product>().WithMany().HasForeignKey(x => x.MergedIntoId);
            modelBuilder.Entity<Product>().Property(x => x.Barcode).HasMaxLength(100);
            modelBuilder.Entity<Product>().Property(x => x.Sku).HasMaxLength(100);
            modelBuilder.Entity<Product>().HasIndex(x => new { x.OrganizationId, x.Barcode }).IsUnique().HasFilter("[Barcode] IS NOT NULL");
            modelBuilder.Entity<Product>().HasIndex(x => new { x.OrganizationId, x.Sku }).IsUnique();
            modelBuilder.Entity<StoreStock>().HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId);
            modelBuilder.Entity<StoreStock>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId);
            modelBuilder.Entity<StoreStock>().HasIndex(x => new { x.LocationId, x.ProductId }).IsUnique();
            modelBuilder.Entity<Customer>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<Customer>().HasIndex(x => new { x.OrganizationId, x.Phone });
            modelBuilder.Entity<Shift>().HasOne<Register>().WithMany().HasForeignKey(x => x.RegisterId);
            modelBuilder.Entity<Shift>().HasOne<User>().WithMany().HasForeignKey(x => x.CashierId);
            modelBuilder.Entity<Shift>().HasIndex(x => x.RegisterId).IsUnique().HasFilter("[ClosedOn] IS NULL");
            modelBuilder.Entity<Shift>().HasIndex(x => x.CashierId).IsUnique().HasFilter("[ClosedOn] IS NULL");
            modelBuilder.Entity<Cart>().HasOne<Shift>().WithMany().HasForeignKey(x => x.ShiftId);
            modelBuilder.Entity<Cart>().HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId);
            modelBuilder.Entity<Cart>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.CartId);
            modelBuilder.Entity<CartLine>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId);
            modelBuilder.Entity<CartLine>().HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
            modelBuilder.Entity<Sale>().HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId);
            modelBuilder.Entity<Sale>().HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId);
            modelBuilder.Entity<Sale>().HasOne<Register>().WithMany().HasForeignKey(x => x.RegisterId);
            modelBuilder.Entity<Sale>().HasOne<Shift>().WithMany().HasForeignKey(x => x.ShiftId);
            modelBuilder.Entity<Sale>().HasOne<Cart>().WithMany().HasForeignKey(x => x.CartId);
            modelBuilder.Entity<Sale>().HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId);
            modelBuilder.Entity<Sale>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SaleId);
            modelBuilder.Entity<Sale>().HasMany(x => x.Payments).WithOne().HasForeignKey(x => x.SaleId);
            modelBuilder.Entity<Sale>().HasIndex(x => x.CartId).IsUnique();
            modelBuilder.Entity<Sale>().HasIndex(x => x.ReceiptNumber).IsUnique().HasFilter("[ReceiptNumber] IS NOT NULL");
            modelBuilder.Entity<Sale>().HasIndex(x => new { x.OrganizationId, x.LocationId, x.BusinessDate });
            modelBuilder.Entity<SaleLine>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId);
            modelBuilder.Entity<SaleLine>().HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId);
            modelBuilder.Entity<Refund>().HasOne<Sale>().WithMany().HasForeignKey(x => x.SaleId);
            modelBuilder.Entity<Refund>().HasOne<Shift>().WithMany().HasForeignKey(x => x.ShiftId);
            modelBuilder.Entity<Refund>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.RefundId);
            modelBuilder.Entity<RefundLine>().HasOne<SaleLine>().WithMany().HasForeignKey(x => x.SaleLineId);
            modelBuilder.Entity<AuditEvent>().HasIndex(x => new { x.OrganizationId, x.LocationId, x.CreatedOn });
            modelBuilder.Entity<Operation>().Property(x => x.Key).HasMaxLength(180);
            modelBuilder.Entity<Operation>().Property(x => x.ResponseJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<Operation>().HasIndex(x => new { x.OrganizationId, x.ActorId, x.Key }).IsUnique();
            modelBuilder.Entity<OutboxMessage>().Property(x => x.Payload).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<OutboxMessage>().HasIndex(x => new { x.Type, x.AggregateId }).IsUnique();
            modelBuilder.Entity<DayClose>().HasOne<Location>().WithMany().HasForeignKey(x => x.LocationId);
            modelBuilder.Entity<DayClose>().HasIndex(x => new { x.LocationId, x.BusinessDate }).IsUnique();
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            modelBuilder.Entity<Cart>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(x => x.Locations).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Entity>().Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity is AuditEvent or OutboxMessage or Operation or Payment or Refund or RefundLine or DayClose)
                    throw new InvalidOperationException("Immutable records cannot be modified.");
                entry.Entity.ModifiedOn = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
