using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;
using Xunit;

namespace UnifiedPOS.Tests;

public class EntityTrackingTests
{
    [Fact]
    public void NewCartLineWithApplicationAssignedIdIsInserted()
    {
        using var context = new POSContext(new DbContextOptionsBuilder<POSContext>().UseSqlServer().Options);
        var cart = new Cart { Version = [1] };
        context.Attach(cart);
        var line = new CartLine { CartId = cart.Id, ProductId = Guid.NewGuid(), Quantity = 1 };
        cart.Lines.Add(line);
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Added, context.Entry(line).State);
    }
}
