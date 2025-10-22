using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AuthApiBackend.RegisterServices
{
    public static class OAuthMiddleware
    {

        public static IServiceCollection OathServiceMiddleware(this IServiceCollection services)
        {

            services.AddAuthentication(options =>
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

            return services;
        }

    }

}
