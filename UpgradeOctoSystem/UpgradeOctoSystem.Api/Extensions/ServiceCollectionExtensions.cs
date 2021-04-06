using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using UpgradeOctoSystem.Abstractions;
using UpgradeOctoSystem.Abstractions.Models;
using UpgradeOctoSystem.Abstractions.Services;
using UpgradeOctoSystem.Api;
using UpgradeOctoSystem.Api.Models;
using UpgradeOctoSystem.Api.Services;
using UpgradeOctoSystem.Database;
using UpgradeOctoSystem.Database.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredAutomapper(this IServiceCollection services)
        {
            services.AddAutoMapper(x =>
            {
                x.CreateMap<IApplicationUser, ApplicationUser>().ReverseMap();
                x.CreateMap<IAuthResponse, AuthResponse>().ReverseMap();
                //x.CreateMap<IApplicationUser, UserResponse>();
            }, typeof(Startup).Assembly);

            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<DatabaseContext>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient(provider => provider.GetService<IHttpContextAccessor>().HttpContext?.User);

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
                options.Lockout.MaxFailedAccessAttempts = 10;

                // Default SignIn settings.
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["AuthSettings:Audience"],
                    ValidIssuer = configuration["AuthSettings:Issuer"],
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthSettings:Key"])),
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddScoped<IForgotPasswordProvider, AuthService>();
            services.AddScoped<ITokenProvider, AuthService>();
            services.AddScoped<IUserProvider, AuthService>();
            return services;
        }

        public static void VerifyInjectionDependencies(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            foreach (var service in services)
            {
                if (service.ImplementationType == null ||
                    !(service!.ImplementationType.FullName?.StartsWith("HusenseRTP") ?? false))
                    continue; // Injection outside our scope
                var constructors = service.ImplementationType.GetConstructors();

                // In case of multiple constructors, service collection will default to the one with the most parameters.
                var constructor = constructors.OrderByDescending(x => x.GetParameters().Length).First();
                if (!constructor.GetParameters().Any())
                    continue; // No injections, all good.
                if (!constructor.GetParameters().All(x => x.ParameterType.IsInterface))
                    continue; // Skip non-interface only injections, as those require additional knowledge to validate
                try
                {
                    foreach (var param in constructor.GetParameters())
                    {
                        if ((param.ParameterType.FullName ?? "").StartsWith("Microsoft.Extensions.Logging.ILogger"))
                            continue; // Azure logger injection
                        provider.GetRequiredService(param.ParameterType);
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    // TODO: Fix recursive service requirements.
                    // As it stands, on calling 'getrequiredservice', all underlying services are interpreted.
                    // Not all the dependencies in those lower relations are in our control, hence we would need to manually recurse the deps
                    // instead of using the pre-provided getrequired services
                    if (ioe.Message.Contains("'Microsoft.Azure.WebJobs.Script.Diagnostics.HostFileLoggerProvider'"))
                        return;
                    Console.WriteLine($"{ioe.Message} For service {service.ImplementationType.FullName} Which requires it.");
                }
            }
        }
    }
}