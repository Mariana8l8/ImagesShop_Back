using Microsoft.Extensions.DependencyInjection;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.Services;

namespace ImagesShop.Application.DependencyInjection
{
    public static class ApplicationCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<IPurchaseHistoryService, PurchaseHistoryService>();

            return services;
        }
    }
}