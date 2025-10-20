using AuthApiBackend.Configurations;
using AuthApiBackend.Controllers.v1;
using AuthApiBackend.Controllers.v2;
using AuthApiBackend.Database;
using AuthApiBackend.Exceptions;
using AuthApiBackend.RegisterServices;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        try
        {
            config.ReadFrom.Configuration(context.Configuration).
            Enrich.FromLogContext().WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel:LogEventLevel.Information);
        }catch(Exception ex)
        {
           Console.WriteLine(ex.Message);
        }
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

});

builder.Services.AddServiceCollection();
Env.Load();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();

    options.Conventions.Controller<HomeController>().HasApiVersion(new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0));
    options.Conventions.Controller<AdminController>().HasApiVersion(new Microsoft.AspNetCore.Mvc.ApiVersion(2, 0));
});

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetRequiredSection("connectionString"));
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));
builder.Services.Configure<MaxAttemptsConfig>(builder.Configuration.GetSection("MaxAttempts"));

builder.Services.AddDbContext<AuthApiDbContext>();

RedisConfig redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfig>()!;

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig.Configuration;
    options.InstanceName = redisConfig.InstanceName;
});

builder.Services.AddSingleton<IExceptionHandler, ExceptionsGlobalHandler>();
builder.Services.AddSingleton<IExceptionHandler, UnknownExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
