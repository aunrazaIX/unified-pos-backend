using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnifiedPOS.Application;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Application.Services;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure;
using UnifiedPOS.Infrastructure.Databases;
using Xunit;

namespace UnifiedPOS.Tests
{
    public class ArchitectureTests
    {
        [Fact]
        public void ApplicationServicesAreRegisteredThroughScopedInterfaces()
        {
            var registrations = new ServiceCollection().AddApplicationLayer();
            var services = typeof(AuthService).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract &&
                    (typeof(ServiceBase).IsAssignableFrom(type) || type == typeof(AuthService)))
                .ToArray();

            Assert.Equal(10, services.Length);
            foreach (var implementation in services)
            {
                var contract = Assert.Single(implementation.GetInterfaces(), type => type.Name == "I" + implementation.Name);
                var registration = Assert.Single(registrations, item => item.ServiceType == contract);
                Assert.Equal(implementation, registration.ImplementationType);
                Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
                Assert.DoesNotContain(registrations, item => item.ServiceType == implementation);
            }
        }

        [Fact]
        public void UnitOfWorkCachesNamedRepositoryInstances()
        {
            using var context = CreateContext();
            IUnitOfWork unitOfWork = new UnitOfWork(context);
            var properties = typeof(IUnitOfWork).GetProperties();

            Assert.Equal(11, properties.Length);
            foreach (var property in properties)
            {
                Assert.True(property.PropertyType.IsInterface);
                Assert.False(property.PropertyType.IsGenericType);
                Assert.EndsWith("Repository", property.Name);
                var repository = property.GetValue(unitOfWork);
                Assert.NotNull(repository);
                Assert.IsAssignableFrom(property.PropertyType, repository);
                Assert.Same(repository, property.GetValue(unitOfWork));
            }
        }

        [Fact]
        public void SqlServerModelContainsAllDomainEntitiesAndConcurrencyTokens()
        {
            using var context = CreateContext();
            var entities = typeof(Entity).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(Entity).IsAssignableFrom(type));

            foreach (var type in entities)
            {
                var mapping = context.Model.FindEntityType(type);
                Assert.NotNull(mapping);
                Assert.NotNull(mapping.FindPrimaryKey());
                var version = mapping.FindProperty(nameof(Entity.Version));
                Assert.NotNull(version);
                Assert.True(version.IsConcurrencyToken);
            }
            Assert.NotNull(context.Model.FindEntityType(typeof(UserLocation)));
        }

        private static POSContext CreateContext() => new(new DbContextOptionsBuilder<POSContext>()
            .UseSqlServer()
            .Options);
    }
}
