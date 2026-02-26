using ImagesShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.API.Middleware
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var method = context.Request.Method.ToUpper();
            
            if (method == "GET" || method == "HEAD" || method == "OPTIONS" || method == "TRACE")
            {
                await _next(context);
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await _next(context);
                
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }
    }
}
