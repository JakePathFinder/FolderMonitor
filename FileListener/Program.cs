using Common.ExtensionMethods;
using FileListener.Cfg;
using FileListener.Constants;
using FileListener.Repos;
using FileListener.Repos.Interfaces;
using FileListener.Services;
using FileListener.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(Const.AppConfigCfgName));
builder.AddVaronisServices();

builder.Services.AddSingleton<IFolderMonitoringService, FolderMonitoringService>();
builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<IFolderMonitoringService>());
builder.Services.AddSingleton<IDistributedSetRepo, FolderRepo>();
builder.Services.AddScoped<IFolderService, FolderService>();

var app = builder.Build();

app.UseVaronisServices();

app.Run();
