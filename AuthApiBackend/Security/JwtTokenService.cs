using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AuthApiBackend.Security
{
    
    public static class JwtTokenService
    {
        
        public static IServiceCollection AddVerifyJWT(this IServiceCollection Service, IConfiguration config)
        {
            Service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = config["JWT:Issuer"],
                    ValidAudience = config["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT_KEY")!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var blackListService = context.HttpContext.RequestServices.GetRequiredService<IBlackListedTokenService>();

                        if (tokenId != null && blackListService.TokenExist(tokenId, CancellationToken.None).Result)
                        {
                            context.Fail("This token has been revoked.");
                        }

                        return Task.CompletedTask;
                    }, 

                    OnMessageReceived = context =>
                    {
                        var token = context.HttpContext.Request.Cookies["token"];

                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine(context.Exception.Message);

                        return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                       context.Response.StatusCode = StatusCodes.Status403Forbidden;
                       context.Response.WriteAsJsonAsync("You are not authorized to access this resource.");

                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.WriteAsJsonAsync("Token Validation Failed");

                        return Task.CompletedTask;
                    }

                };

            });

            return Service;
        }

    }

}
