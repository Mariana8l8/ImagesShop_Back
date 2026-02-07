using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Infrastructure.Data;
using ImagesShop.Infrastructure.Repositories;
using ImagesShop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImagesShop.Infrastructure.DependencyInjection
{
    public static class InfrastructureCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("DefaultConnection is missing or empty.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly("ImagesShop.Infrastructure")));

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPurchaseHistoryRepository, PurchaseHistoryRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IUserTransactionRepository, UserTransactionRepository>();
            services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
            services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();

            services.AddScoped<IEmailSender, SmtpEmailSender>();

            return services;
        }
    }
}