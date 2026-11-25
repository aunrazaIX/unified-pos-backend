using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UnifiedPOS.API.Controllers;
using UnifiedPOS.API.Infrastructure;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using Xunit;

namespace UnifiedPOS.Tests;

public class ApiContractTests
{
    private static readonly Type[] Controllers = typeof(AuthController).Assembly.GetTypes()
        .Where(x => x.IsClass && !x.IsAbstract && typeof(ControllerBase).IsAssignableFrom(x)).ToArray();

    [Fact]
    public void AllControllersUseApiControllerAndVersionedRoutes()
    {
        Assert.Equal(10, Controllers.Length);
        foreach (var controller in Controllers)
        {
            Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
            var route = Assert.Single(controller.GetCustomAttributes<RouteAttribute>()).Template;
            Assert.StartsWith("api/v1", route);
        }
    }

    [Fact]
    public void OnlyAuthenticationEntryPointsAreAnonymous()
    {
        var anonymous = Controllers.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                .Select(method => $"{type.Name}.{method.Name}"))
            .OrderBy(x => x).ToArray();
        Assert.Equal(["AuthController.Login", "AuthController.Pin", "AuthController.Refresh"], anonymous);
        foreach (var controller in Controllers.Where(x => x != typeof(AuthController)))
            Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(typeof(AuthController).GetMethod(nameof(AuthController.Logout))!.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void EveryMutationExceptAuthenticationAndDisabledSyncRequiresIdempotencyKey()
    {
        var exempt = new HashSet<string> { "AuthController.Login", "AuthController.Pin", "AuthController.Refresh", "AuthController.Logout", "PosController.Sync", "PosController.SelectRegister" };
        var mutations = Controllers.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.GetCustomAttributes().Any(attribute => attribute is HttpPostAttribute or HttpPutAttribute))
            .Select(method => (Name: $"{type.Name}.{method.Name}", Method: method)));
        foreach (var mutation in mutations.Where(x => !exempt.Contains(x.Name)))
        {
            var key = Assert.Single(mutation.Method.GetParameters(), parameter =>
                parameter.GetCustomAttribute<FromHeaderAttribute>()?.Name == "Idempotency-Key");
            Assert.Equal(typeof(string), key.ParameterType);
        }
    }

    [Fact]
    public void JwtContainsRequiredIdentityAndSessionClaims()
    {
        var settings = Options.Create(new JwtSettings
        {
            Key = "a-production-length-test-key-which-is-not-a-secret",
            Issuer = "tests", Audience = "tests", AccessMinutes = 10
        });
        var user = new User { Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), Role = UserRole.Manager };
        var session = new Session { Id = Guid.NewGuid(), UserId = user.Id, RegisterId = Guid.NewGuid() };
        var response = new JwtService(settings).Create(user, session, "refresh");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);

        Assert.Equal(user.Id.ToString(), token.Claims.Single(x => x.Type == JwtClaims.UserId).Value);
        Assert.Equal(user.OrganizationId.ToString(), token.Claims.Single(x => x.Type == JwtClaims.OrganizationId).Value);
        Assert.Equal(session.Id.ToString(), token.Claims.Single(x => x.Type == JwtClaims.SessionId).Value);
        Assert.Equal(session.RegisterId.ToString(), token.Claims.Single(x => x.Type == JwtClaims.RegisterId).Value);
        Assert.Equal(UserRole.Manager.ToString(), token.Claims.Single(x => x.Type == JwtClaims.Role).Value);
        Assert.Equal("refresh", response.RefreshToken);
    }
}
