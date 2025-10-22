using AuthApiBackend.Configurations;
using AuthApiBackend.Controllers.v1;
using AuthApiBackend.Controllers.v2;
using AuthApiBackend.Database;
using AuthApiBackend.Exceptions;
using AuthApiBackend.RegisterServices;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    { 
            config.ReadFrom.Configuration(context.Configuration).
            Enrich.FromLogContext().WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel:LogEventLevel.Information);
    });

try
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {

        options.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID")!;
        options.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
            
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.CallbackPath = "/signing-google/google";
       
        options.AccessType = "online";
        
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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}catch(Exception ex)
{
    Log.Fatal("{Exception} was thrown", ex);
}
finally
{
    await Log.CloseAndFlushAsync();
}