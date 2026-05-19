using Hospital.Catalog.Api.Data;
using Hospital.Catalog.Api.Dtos;
using Hospital.Catalog.Api.Models;
using Hospital.Catalog.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Hospital.Catalog.Tests;

public class DoctorServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsDoctor_AndClearsCache()
    {
        await using var context = CreateContext();
        var cache = CreateCacheMock();
        var service = new DoctorService(context, cache.Object);

        var result = await service.CreateAsync(new CreateDoctorDto
        {
            FullName = "Dr. Sarah Lee",
            Specialization = "Cardiology",
            Department = "Heart Center"
        });

        Assert.True(result.Id > 0);
        Assert.True(result.IsAvailable);
        Assert.Equal("Dr. Sarah Lee", result.FullName);
        cache.Verify(item => item.RemoveAsync("catalog:doctors:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FilterAsync_AppliesSearchAvailabilityAndSorting()
    {
        await using var context = CreateContext();
        var cache = CreateCacheMock();
        context.Doctors.AddRange(
            new Doctor
            {
                FullName = "Dr. Alex Kim",
                Specialization = "Cardiology",
                Department = "Heart Center",
                IsAvailable = true
            },
            new Doctor
            {
                FullName = "Dr. Sam Green",
                Specialization = "Neurology",
                Department = "Brain Center",
                IsAvailable = true
            },
            new Doctor
            {
                FullName = "Dr. Maria Stone",
                Specialization = "Cardiology",
                Department = "Heart Center",
                IsAvailable = false
            });
        await context.SaveChangesAsync();

        var service = new DoctorService(context, cache.Object);

        var result = await service.FilterAsync(
            specialization: "card",
            department: "heart",
            available: true,
            search: "alex",
            sortBy: "specialization",
            descending: false);

        var doctor = Assert.Single(result);
        Assert.Equal("Dr. Alex Kim", doctor.FullName);
    }

    private static CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CatalogDbContext(options);
    }

    private static Mock<IDistributedCache> CreateCacheMock()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(item => item.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        cache.Setup(item => item.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        cache.Setup(item => item.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return cache;
    }
}
