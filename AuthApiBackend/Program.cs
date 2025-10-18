using AuthApiBackend.Configurations;
using AuthApiBackend.Controllers.v1;
using AuthApiBackend.Controllers.v2;
using AuthApiBackend.Database;
using AuthApiBackend.RegisterServices;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
