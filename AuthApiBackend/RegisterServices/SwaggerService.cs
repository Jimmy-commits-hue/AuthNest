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

            });

            return services;

        }

    }

}
