// <copyright file="AdminServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SurveyPro.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SurveyPro.Application.Configuration;
using SurveyPro.Infrastructure.Services;
using SurveyPro.Domain.Entities;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AdminUserService"/>.
/// </summary>
public class AdminUserServiceTests
{
    private static AdminUserService BuildService(
        UserManager<ApplicationUser> userManager,
        IMemoryCache? cache = null,
        int cacheMinutes = 10)
    {
        cache ??= new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new CacheSettings { UsersListExpirationMinutes = cacheMinutes });
        var logger = new Mock<ILogger<AdminUserService>>();
        return new AdminUserService(userManager, cache, options, logger.Object);
    }

    private static UserManager<ApplicationUser> CreateUserManager(IList<ApplicationUser> users)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        mgr.Setup(m => m.Users).Returns(users.AsQueryable());
        mgr.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        return mgr.Object;
    }

    private static ApplicationUser MakeUser(bool isBlocked = false) => new ApplicationUser
    {
        Id = Guid.NewGuid(),
        Name = "Test User",
        Email = "test@example.com",
        IsBlocked = isBlocked,
        CreatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetUsersAsync_ReturnsAllUsers()
    {
        var users = new List<ApplicationUser> { MakeUser(), MakeUser() };
        var service = BuildService(CreateUserManager(users));

        var result = await service.GetUsersAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsersAsync_MapsUserFieldsCorrectly()
    {
        var user = MakeUser();
        var service = BuildService(CreateUserManager(new List<ApplicationUser> { user }));

        var result = await service.GetUsersAsync(CancellationToken.None);
        var dto = result.First();

        dto.Id.Should().Be(user.Id);
        dto.Name.Should().Be(user.Name);
        dto.Email.Should().Be(user.Email);
        dto.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersAsync_MapsRolesCorrectly()
    {
        var user = MakeUser();
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.Users).Returns(new List<ApplicationUser> { user }.AsQueryable());
        mgr.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin", "User" });

        var service = BuildService(mgr.Object);

        var result = await service.GetUsersAsync(CancellationToken.None);

        result.First().Roles.Should().BeEquivalentTo(new[] { "Admin", "User" });
    }

    [Fact]
    public async Task GetUsersAsync_SecondCall_ReturnsCachedResult()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { MakeUser() }.AsQueryable());
        mgr.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(mgr.Object, cache);

        await service.GetUsersAsync(CancellationToken.None);
        await service.GetUsersAsync(CancellationToken.None);

        // Users property on UserManager should only be accessed once (first call),
        // second call is served from cache, so GetRolesAsync is also called once.
        mgr.Verify(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_EmptyUserList_ReturnsEmptyCollection()
    {
        var service = BuildService(CreateUserManager(new List<ApplicationUser>()));

        var result = await service.GetUsersAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BlockUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var service = BuildService(mgr.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.BlockUserAsync(Guid.NewGuid().ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task BlockUserAsync_ValidUser_SetsIsBlockedTrue()
    {
        var user = MakeUser(isBlocked: false);
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = BuildService(mgr.Object);

        await service.BlockUserAsync(user.Id.ToString(), CancellationToken.None);

        user.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task BlockUserAsync_ValidUser_InvalidatesCache()
    {
        var user = MakeUser();
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable());
        mgr.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(mgr.Object, cache);

        // Populate cache
        await service.GetUsersAsync(CancellationToken.None);

        // Block user — should evict cache
        await service.BlockUserAsync(user.Id.ToString(), CancellationToken.None);

        // GetUsersAsync should hit DB again (GetRolesAsync called a second time)
        await service.GetUsersAsync(CancellationToken.None);
        mgr.Verify(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(2));
    }

    [Fact]
    public async Task BlockUserAsync_UpdateFails_ThrowsInvalidOperationException()
    {
        var user = MakeUser();
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var service = BuildService(mgr.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.BlockUserAsync(user.Id.ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task UnblockUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var service = BuildService(mgr.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UnblockUserAsync(Guid.NewGuid().ToString(), CancellationToken.None));
    }

    [Fact]
    public async Task UnblockUserAsync_ValidUser_SetsIsBlockedFalse()
    {
        var user = MakeUser(isBlocked: true);
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = BuildService(mgr.Object);

        await service.UnblockUserAsync(user.Id.ToString(), CancellationToken.None);

        user.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task UnblockUserAsync_ValidUser_InvalidatesCache()
    {
        var user = MakeUser(isBlocked: true);
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.Users)
            .Returns(new List<ApplicationUser> { user }.AsQueryable());
        mgr.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = BuildService(mgr.Object, cache);

        await service.GetUsersAsync(CancellationToken.None);
        await service.UnblockUserAsync(user.Id.ToString(), CancellationToken.None);
        await service.GetUsersAsync(CancellationToken.None);

        mgr.Verify(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UnblockUserAsync_UpdateFails_ThrowsInvalidOperationException()
    {
        var user = MakeUser(isBlocked: true);
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mgr.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var service = BuildService(mgr.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UnblockUserAsync(user.Id.ToString(), CancellationToken.None));
    }
}