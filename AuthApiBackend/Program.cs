using AuthApiBackend.Configurations;
using AuthApiBackend.Database;
using AuthApiBackend.Exceptions;
using AuthApiBackend.RegisterServices;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;
using Serilog.Events;

Env.Load();

Console.WriteLine(Environment.GetEnvironmentVariable("KEY"));
var builder = WebApplication.CreateBuilder(args);

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

    builder.Services.OathServiceMiddleware();

    builder.Services.AddServiceCollection();

    //Binding appsettingsjson to Dtos
    builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetRequiredSection("connectionString"));
    builder.Services.Configure<EmailConfig>(builder.Configuration.GetRequiredSection("EmailConfig"));
    builder.Services.Configure<MaxAttemptsConfig>(builder.Configuration.GetRequiredSection("MaxAttempts"));

    builder.Services.AddDbContext<AuthApiDbContext>();

    builder.Services.AddSingleton<IExceptionHandler, ExceptionsGlobalHandler>();
    builder.Services.AddSingleton<IExceptionHandler, UnknownExceptionHandler>();

    var app = builder.Build();

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

    await Log.CloseAndFlushAsync();

}