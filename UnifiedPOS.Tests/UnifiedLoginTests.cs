using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.DTOs;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;
using Xunit;

namespace UnifiedPOS.Tests;

public class UnifiedLoginTests
{
    [Fact]
    public void LoginDoesNotRequireOrganizationIdentity()
    {
        Assert.Null(typeof(AuthDTO.LoginRequest).GetProperty("OrganizationId"));
        Assert.NotNull(typeof(AuthDTO.LoginRequest).GetProperty("Password"));
    }

    [Fact]
    public void DatabaseEnforcesGlobalUsernameUniquenessAndRegisterAssignmentIntegrity()
    {
        using var context = new POSContext(new DbContextOptionsBuilder<POSContext>().UseSqlServer().Options);
        var user = context.Model.FindEntityType(typeof(User))!;
        Assert.Contains(user.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(["NormalizedUserName"]));
        var assignment = context.Model.FindEntityType(typeof(UserRegister))!;
        Assert.Equal(new[] { "UserId", "RegisterId" }, assignment.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.Equal(2, assignment.GetForeignKeys().Count());
    }
}
