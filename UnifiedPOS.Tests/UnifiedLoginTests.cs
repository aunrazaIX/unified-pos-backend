using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.DTOs;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;
using Xunit;

namespace UnifiedPOS.Tests;

public class UnifiedLoginTests
{
    [Fact]
    public void LoginAcceptsExistingShortPasswordButCreationRetainsPolicy()
    {
        var login = new AuthDTO.LoginRequest { UserName = "existing-user", Password = "123456789" };
        var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        Assert.True(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(login,
            new System.ComponentModel.DataAnnotations.ValidationContext(login), errors, true));
        var create = new AuthDTO.UserRequest { UserName = "new-user", Password = "123456789" };
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(create,
            new System.ComponentModel.DataAnnotations.ValidationContext(create), errors, true));
    }

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
