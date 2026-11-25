using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Bootstrap;
using UnifiedPOS.Infrastructure.Databases;
using Xunit;

namespace UnifiedPOS.Tests;

public class BootstrapTests
{
    [Fact]
    public async Task DevelopmentPasswordExceptionCannotBeEnabledInProduction()
    {
        var options = ValidOptions();
        options.OwnerPassword = new string('x', 9);
        options.AllowDevelopmentShortPassword = true;
        options.AllowProduction = true;
        Assert.True(options.IsValid());
        using var context = new POSContext(new DbContextOptionsBuilder<POSContext>().UseSqlServer().Options);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new OwnerProvisioner(context, new PasswordHasher<User>(), NullLogger<OwnerProvisioner>.Instance)
                .ProvisionAsync(options, false));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(8, false)]
    [InlineData(11, false)]
    [InlineData(12, true)]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public void EnabledBootstrapEnforcesPasswordPolicy(int length, bool expected)
    {
        var options = ValidOptions();
        options.OwnerPassword = new string('x', length);
        Assert.Equal(expected, options.IsValid());
    }

    [Fact]
    public async Task DisabledBootstrapDoesNotAccessDatabase()
    {
        using var context = new POSContext(new DbContextOptionsBuilder<POSContext>().UseSqlServer().Options);
        await new OwnerProvisioner(context, new PasswordHasher<User>(), NullLogger<OwnerProvisioner>.Instance)
            .ProvisionAsync(new BootstrapOptions(), false);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task ProductionBootstrapRequiresExplicitOptIn()
    {
        using var context = new POSContext(new DbContextOptionsBuilder<POSContext>().UseSqlServer().Options);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new OwnerProvisioner(context, new PasswordHasher<User>(), NullLogger<OwnerProvisioner>.Instance)
                .ProvisionAsync(ValidOptions(), false));
    }

    [Fact]
    public void MissingConfigurationFailsValidation()
    {
        Assert.False(new BootstrapOptions { Enabled = true }.IsValid());
    }

    private static BootstrapOptions ValidOptions() => new()
    {
        Enabled = true, OrganizationName = "Test organization", Currency = "PKR",
        OwnerUserName = "owner@example.test", OwnerPassword = new string('x', 12)
    };
}
