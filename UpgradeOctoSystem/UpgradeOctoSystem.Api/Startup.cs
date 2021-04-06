using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Extensions;
using UpgradeOctoSystem.Abstractions.Services;
using UpgradeOctoSystem.Api.Models;
using UpgradeOctoSystem.Api.Services;
using UpgradeOctoSystem.Database;

namespace UpgradeOctoSystem.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                .AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name
                    && level == LogLevel.Information);
            });

            services
                .AddControllers()
                .AddJsonOptions(
                    options =>
                    {
                        options.JsonSerializerOptions.MaxDepth = 32;
                        options.JsonSerializerOptions.AllowTrailingCommas = true;
                    });

            services
                .AddAuth(_configuration)
                .AddConfiguredAutomapper();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Octo_Api", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    Description = "Please insert JWT token into field"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:3000")
                        .AllowCredentials();
                });
            });

            services.AddDbContext<DatabaseContext>(options =>
                options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer("Data Source=.;Initial Catalog=UpgradedOcto;Integrated Security=True") // TODO: abstract to config
                    .EnableSensitiveDataLogging(),
                //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
                ServiceLifetime.Transient,
                ServiceLifetime.Transient
            );

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ctx =>
                {
                    var exception = new ValidationException(ExceptionMapping.None, "", ctx.ModelState);
                    try
                    {
                        ctx.HttpContext.Response.StatusCode = 400;
                        return new BadRequestObjectResult(exception.ToErrorResponse());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    return new BadRequestObjectResult(exception.ToErrorResponse());
                };
            });

            services
                .AddScoped<IBlobStorageService, BlobStorageService>()
                .AddScoped<IMailerService, MailerService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;
                var code = 500; // Internal Server Error by default
                context.Response.StatusCode = code; // You can use HttpStatusCode enum instead

                if (exception is ValidationException validationException)
                {
                    try
                    {
                        code = 400;
                        await context.Response.WriteAsync(validationException.ToErrorResponse().ToJson());
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

#if DEBUG
                await context.Response.WriteAsync(new { error = exception.Message }.ToJson());
#else
                await context.Response.WriteAsync(new { error = "An unhandled exception occured" }.ToJson());
#endif
            }));

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Octo_API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseCors("ClientPermission");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
