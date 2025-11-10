using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace AuthApiBackend.RegisterServices
{

    public static class SwaggerService
    {

        public static IServiceCollection AddSwaggerService(this IServiceCollection services)
        {

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {

                var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in provider.ApiVersionDescriptions)
                {

                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {

                        Title = $"AuthApiBackend {description.ApiVersion}",
                        Version = description.GroupName

                    });

                }

                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "token", // this is your cookie name
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Description = "JWT token stored in HttpOnly cookie named 'token'."
                });

                // ✅ Add requirement globally
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                In = ParameterLocation.Cookie
            },
            new string[] {}
        }
    });
            });

            return services;

        }

    }

}