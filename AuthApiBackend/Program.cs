using AuthApiBackend.Configurations;
using AuthApiBackend.Database;
using AuthApiBackend.Exceptions;
using AuthApiBackend.RegisterServices;
using AuthApiBackend.Security;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;



var builder = WebApplication.CreateBuilder(args);

Env.Load();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console().WriteTo.Seq("http://localhost:5341")
    .MinimumLevel.Debug()
    .CreateLogger();

try
{
    builder.Host.UseSerilog((context, services, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration).
        Enrich.FromLogContext().WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Information);
    });

    builder.Services.AddSwaggerService();

    builder.Services.AddControllers().AddNewtonsoftJson(options => {
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    });

    builder.Services.AddServiceCollection();
    builder.Services.AddVerifyJWT(builder.Configuration);
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetRequiredSection("connectionString"));
    builder.Services.Configure<EmailConfig>(builder.Configuration.GetRequiredSection("EmailConfig"));
    builder.Services.Configure<MaxAttemptsConfig>(builder.Configuration.GetRequiredSection("MaxAttempts"));
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetRequiredSection("JWT"));

    var jwt = builder.Configuration.GetSection("JWT").Get<JwtConfig>();

    builder.Services.AddDbContext<AuthApiDbContext>();

    builder.Services.AddSingleton<IExceptionHandler, ExceptionsGlobalHandler>();
    builder.Services.AddSingleton<IExceptionHandler, UnknownExceptionHandler>();
    
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy =>
        {
            policy.RequireRole("Admin");
        });

        options.AddPolicy("User", policy =>
        {
            policy.RequireRole("User");
        });
    });

    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
    });

    builder.Services.AddRateLimiter(options =>
    { 
        options.AddPolicy("BeforeLogin", context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!context.Request.HttpContext.Request.Cookies.TryGetValue("partitionKey", out var token))
            {
                var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ip + Guid.NewGuid().ToString()));
                token = Convert.ToBase64String(hash);

                context.Request.HttpContext.Response.Cookies.Append("partitionKey", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(10)
                });
            }

            string partitionKey = token;

            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,                
                Window = TimeSpan.FromMinutes(1), 
                SegmentsPerWindow = 2,  
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });

        options.AddPolicy("AfterLogin", context =>
        {

            if(context.Request.HttpContext.Request.Cookies.TryGetValue("partitionKey", out var oldCookie))
            {
                context.Request.HttpContext.Response.Cookies.Delete("partitionKey");
            }

            if (!context.Request.HttpContext.Request.Cookies.TryGetValue("afterloginPartitionKey", out var token))
            {
                token = $"{context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "UNKNOWN"}";

                context.Request.HttpContext.Response.Cookies.Append("afterloginPartitionKey", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
            }

            return RateLimitPartition.GetTokenBucketLimiter(token, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,
                TokensPerPeriod = 1,
                ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsJsonAsync($"Too many request, Please slow down.", cancellationToken);
        };
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = (AuthApiDbContext)scope.ServiceProvider.GetRequiredService<AuthApiDbContext>();
        var retryCount = 5;
        Console.WriteLine("Applying database migrations...");
        while (retryCount > 0)
        {
            try
            {
                db.Database.Migrate();
                break;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"DB not ready, retrying in 5 seconds...{ex.Message}");
                Thread.Sleep(5000);
                retryCount--;
            }
        }

        if (!db.Database.CanConnect())
        {
            Environment.Exit(1);
        }
    }

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"API {description.GroupName.ToUpperInvariant()}");
            }
        });

    }

    app.UseRateLimiter();
    app.UseExceptionHandler(_ => { });
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal("{Exception} was thrown", ex);
}
finally
{
    Log.CloseAndFlush();
}