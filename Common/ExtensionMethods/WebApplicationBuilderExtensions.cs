using Common.Cfg;
using Common.Constants;
using Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace Common.ExtensionMethods
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddVaronisServices(this WebApplicationBuilder builder)
        {
            var cfgSection = builder.Configuration.GetSection(Const.CommonConfigCfgName);
            var commonCfg = cfgSection.Get<CommonConfig>() ?? throw new ArgumentException($"Cannot retrieve {Const.CommonConfigCfgName}");
            
            var swaggerVersion = commonCfg.SwaggerConfig.Version;
            var swaggerTitle = commonCfg.SwaggerConfig.Title;
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(swaggerVersion, new OpenApiInfo { Title = swaggerTitle, Version = swaggerVersion });
                c.UseAllOfForInheritance();
                c.UseInlineDefinitionsForEnums();
                c.EnableAnnotations();
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddLog4Net();
            builder.Logging.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);

            builder.Services.Configure<CommonConfig>(cfgSection);
            builder.Services.AddSingleton<IUtilitiesService,UtilitiesService>();
            builder.Services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            builder.Services.AddSingleton<IMessageQueueService, RabbitMqService>();
            builder.Services.AddSingleton<IRmqConnectionFactory, RmqConnectionFactory>();
            
        }
    }
}
