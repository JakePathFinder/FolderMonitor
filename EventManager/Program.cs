using Common.ExtensionMethods;
using EventManager.Cfg;
using EventManager.Constants;
using EventManager.Repos;
using EventManager.Repos.Interfaces;
using EventManager.Services;
using EventManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(Const.AppConfigCfgName));
builder.Services.AddAutoMapper(typeof(Program));
builder.AddVaronisServices();

builder.Services.AddSingleton<IFileEventRepo, FileEventRepo>();
builder.Services.AddHostedService<FileEventHandlerService>();
builder.Services.AddScoped<IQueryService, QueryService>();

var app = builder.Build();

app.UseVaronisServices();

app.Run();
