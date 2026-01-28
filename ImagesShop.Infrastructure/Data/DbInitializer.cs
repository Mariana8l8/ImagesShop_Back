using ImagesShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImagesShop.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Clear in FK-safe order
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [OrderItems];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [ImageTags];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Purchases];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Orders];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Images];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Tags];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Categories];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Users];", cancellationToken);

                // Create sample data (use deterministic GUIDs)
                var catNature = new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Nature" };
                var catAbstract = new Category { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Abstract" };
                var catPeople = new Category { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "People" };
                var catUrban = new Category { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Urban" };

                var tagSunset = new Tag { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "sunset" };
                var tagMountain = new Tag { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "mountain" };
                var tagPortrait = new Tag { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "portrait" };
                var tagAbstract = new Tag { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "abstract" };
                var tagCity = new Tag { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "city" };

                var userAlice = new User
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Name = "Alice",
                    Email = "alice@example.com",
                    PasswordHash = "init_hash_alice",
                    Balance = 100.00m
                };

                var userBob = new User
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Name = "Bob",
                    Email = "bob@example.com",
                    PasswordHash = "init_hash_bob",
                    Balance = 50.00m
                };

                var imgSunset = new Image
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Golden Sunset",
                    Description = "A beautiful golden sunset.",
                    Price = 9.99m,
                    WatermarkedUrl = "https://cdn.example.com/wm/golden-sunset.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/golden-sunset.jpg",
                    CategoryId = catNature.Id
                };

                var imgAbstract1 = new Image
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Title = "Color Flow",
                    Description = "Abstract flowing colors.",
                    Price = 7.00m,
                    WatermarkedUrl = "https://cdn.example.com/wm/color-flow.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/color-flow.jpg",
                    CategoryId = catAbstract.Id
                };

                var imgCity1 = new Image
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
                    Title = "Night City",
                    Description = "City lights at night.",
                    Price = 6.50m,
                    WatermarkedUrl = "https://cdn.example.com/wm/night-city.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/night-city.jpg",
                    CategoryId = catUrban.Id
                };

                await context.Categories.AddRangeAsync(new[] { catNature, catAbstract, catPeople, catUrban }, cancellationToken);
                await context.Tags.AddRangeAsync(new[] { tagSunset, tagMountain, tagPortrait, tagAbstract, tagCity }, cancellationToken);
                await context.Users.AddRangeAsync(new[] { userAlice, userBob }, cancellationToken);
                await context.Images.AddRangeAsync(new[] { imgSunset, imgAbstract1, imgCity1 }, cancellationToken);

                userAlice.Wishlist.Add(imgSunset);
                userBob.Wishlist.Add(imgAbstract1);

                await context.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}