using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ImagesShop.Infrastructure.Data;
using ImagesShop.Infrastructure.Repositories;
using ImagesShop.Application.Interfaces.IRepositories;

namespace ImagesShop.Infrastructure.DependencyInjection
{
    public static class InfrastructureCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString = null)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString,
                        b => b.MigrationsAssembly("ImagesShop.Infrastructure"))
                );
            }

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPurchaseHistoryRepository, PurchaseHistoryRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}