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
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [OrderItems];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [ImageTags];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Purchases];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Orders];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Images];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Tags];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Categories];", cancellationToken);
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Users];", cancellationToken);

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

                var userCarol = new User
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Name = "Carol",
                    Email = "carol@example.com",
                    PasswordHash = "init_hash_carol",
                    Balance = 25.00m
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

                var imgMountain = new Image
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Title = "Misty Mountain",
                    Description = "Mountain in the morning mist.",
                    Price = 8.50m,
                    WatermarkedUrl = "https://cdn.example.com/wm/misty-mountain.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/misty-mountain.jpg",
                    CategoryId = catNature.Id
                };

                var imgPortrait = new Image
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                    Title = "Portrait Smile",
                    Description = "Warm human portrait.",
                    Price = 5.00m,
                    WatermarkedUrl = "https://cdn.example.com/wm/portrait-smile.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/portrait-smile.jpg",
                    CategoryId = catPeople.Id
                };

                var imgAbstract2 = new Image
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666667"),
                    Title = "Shapes & Hues",
                    Description = "Geometric abstract art.",
                    Price = 6.75m,
                    WatermarkedUrl = "https://cdn.example.com/wm/shapes-hues.jpg",
                    OriginalUrl = "https://cdn.example.com/orig/shapes-hues.jpg",
                    CategoryId = catAbstract.Id
                };

                await context.Categories.AddRangeAsync(new[] { catNature, catAbstract, catPeople, catUrban }, cancellationToken);
                await context.Tags.AddRangeAsync(new[] { tagSunset, tagMountain, tagPortrait, tagAbstract, tagCity }, cancellationToken);
                await context.Users.AddRangeAsync(new[] { userAlice, userBob, userCarol }, cancellationToken);
                await context.Images.AddRangeAsync(new[] { imgSunset, imgAbstract1, imgCity1, imgMountain, imgPortrait, imgAbstract2 }, cancellationToken);

                var it1 = new ImageTag { ImageId = imgSunset.Id, TagId = tagSunset.Id };
                var it2 = new ImageTag { ImageId = imgMountain.Id, TagId = tagMountain.Id };
                var it3 = new ImageTag { ImageId = imgPortrait.Id, TagId = tagPortrait.Id };
                var it4 = new ImageTag { ImageId = imgAbstract1.Id, TagId = tagAbstract.Id };
                var it5 = new ImageTag { ImageId = imgAbstract2.Id, TagId = tagAbstract.Id };
                var it6 = new ImageTag { ImageId = imgCity1.Id, TagId = tagCity.Id };

                await context.ImageTags.AddRangeAsync(new[] { it1, it2, it3, it4, it5, it6 }, cancellationToken);

                userAlice.Wishlist.Add(imgSunset);
                userBob.Wishlist.Add(imgAbstract1);
                userCarol.Wishlist.Add(imgMountain);
                userAlice.Wishlist.Add(imgPortrait);

                var order1 = new Order
                {
                    Id = Guid.Parse("aaaaaaaa-1111-0000-0000-aaaaaaaaaaaa"),
                    UserId = userAlice.Id,
                    TotalAmount = imgSunset.Price + imgPortrait.Price,
                    Currency = "USD",
                    Status = Domain.Enums.OrderStatus.Completed
                };

                var order2 = new Order
                {
                    Id = Guid.Parse("bbbbbbbb-2222-0000-0000-bbbbbbbbbbbb"),
                    UserId = userBob.Id,
                    TotalAmount = imgAbstract1.Price,
                    Currency = "USD",
                    Status = Domain.Enums.OrderStatus.Completed
                };

                var oi1 = new OrderItem
                {
                    Id = Guid.Parse("dddddddd-3333-0000-0000-dddddddddddd"),
                    OrderId = order1.Id,
                    ImageId = imgSunset.Id
                };

                var oi2 = new OrderItem
                {
                    Id = Guid.Parse("eeeeeeee-4444-0000-0000-eeeeeeeeeeee"),
                    OrderId = order1.Id,
                    ImageId = imgPortrait.Id
                };

                var oi3 = new OrderItem
                {
                    Id = Guid.Parse("ffffffff-5555-0000-0000-ffffffffffff"),
                    OrderId = order2.Id,
                    ImageId = imgAbstract1.Id
                };

                await context.Orders.AddRangeAsync(new[] { order1, order2 }, cancellationToken);
                await context.OrderItems.AddRangeAsync(new[] { oi1, oi2, oi3 }, cancellationToken);

                var p1 = new PurchaseHistory
                {
                    Id = Guid.Parse("99999999-aaaa-0000-0000-999999999990"),
                    UserName = userAlice.Name,
                    UserEmail = userAlice.Email,
                    ImageId = imgSunset.Id,
                    ImagePrice = imgSunset.Price,
                    ImageTitle = imgSunset.Title
                };

                var p2 = new PurchaseHistory
                {
                    Id = Guid.Parse("99999999-bbbb-0000-0000-999999999991"),
                    UserName = userBob.Name,
                    UserEmail = userBob.Email,
                    ImageId = imgAbstract1.Id,
                    ImagePrice = imgAbstract1.Price,
                    ImageTitle = imgAbstract1.Title
                };

                await context.Purchases.AddRangeAsync(new[] { p1, p2 }, cancellationToken);

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