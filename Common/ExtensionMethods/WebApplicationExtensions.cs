using Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Common.ExtensionMethods
{
    public static class WebApplicationExtensions
    {
        public static void UseVaronisServices(this WebApplication app) {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMiddleware<UnhandledExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
