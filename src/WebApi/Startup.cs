using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNet.Security.OpenId.Steam;
using AutoMapper;
using Crpg.Application;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Steam;
using Crpg.Application.Users.Commands;
using Crpg.Common.Helpers;
using Crpg.Infrastructure;
using Crpg.Persistence;
using Crpg.WebApi.Middlewares;
using Crpg.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Crpg.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private IServiceCollection? _services;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSection = _configuration.GetSection("Jwt");

            services
                .AddInfrastructure(_configuration, _environment)
                .AddPersistence(_configuration, _environment)
                .AddApplication()
                .AddHttpContextAccessor() // Injects IHttpContextAccessor
                .AddScoped<ICurrentUserService, CurrentUserService>()
                .Configure<JwtConfiguration>(jwtSection)
                .AddSingleton<ITokenIssuer, JwtTokenIssuer>()
                .AddSwaggerGen(ConfigureSwagger)
                .AddCors(ConfigureCors)
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonArrayStringEnumFlagsConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });

            services.AddHealthChecks();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(ConfigureJwtBearer)
                .AddSteam(ConfigureSteamAuthentication);

            _services = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                RegisteredServicesPage(app);
            }
            else if (env.IsProduction())
            {
                // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx#use-a-reverse-proxy-server
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app
                .UseCustomExceptionHandler()
                .UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crpg API"))
                .UseRouting()
                .UseCors()
                .UseAuthentication() // populate HttpContext.User
                .UseAuthorization() // check that HttpContext.User has the correct rights to access the endpoint
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health");
                    endpoints.MapControllers();
                });
        }

        private void RegisteredServicesPage(IApplicationBuilder app)
        {
            app.Map("/services", builder => builder.Run(async context =>
            {
                var sb = new StringBuilder()
                    .Append("<h1>Registered Services</h1>")
                    .Append("<table><thead>")
                    .Append("<tr><th>Type</th><th>Lifetime</th><th>Instance</th></tr>")
                    .Append("</thead><tbody>");
                foreach (var svc in _services!.OrderBy(s => s.ServiceType.FullName))
                {
                    sb.Append("<tr>")
                        .Append($"<td>{svc.ServiceType.FullName}</td>")
                        .Append($"<td>{svc.Lifetime}</td>")
                        .Append($"<td>{svc.ImplementationType?.FullName}</td>")
                        .Append("</tr>");
                }

                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));
        }

        private void ConfigureCors(CorsOptions options)
        {
            string allowedOrigins = _configuration["AllowedOrigins"] ?? string.Empty;
            options.AddDefaultPolicy(builder => builder
                .WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
        }

        private void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Crpg API", Version = "v1" });

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Scheme = "bearer",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new List<string>()
                }
            });
        }

        private void ConfigureJwtBearer(JwtBearerOptions options)
        {
            var secret = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Secret"));

            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secret),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = ctx =>
                {
                    // refresh the token if it is about to expire
                    if (ctx.SecurityToken.ValidTo.Subtract(TimeSpan.FromMinutes(15)) > DateTime.UtcNow)
                    {
                        return Task.CompletedTask;
                    }

                    var tokenIssuer = ctx.HttpContext.RequestServices.GetService<ITokenIssuer>();
                    string token = tokenIssuer.IssueToken(ctx.Principal.Identities.First());
                    ctx.Response.Headers["Refresh-Authorization"] = token;
                    return Task.CompletedTask;
                },
            };
        }

        private void ConfigureSteamAuthentication(SteamAuthenticationOptions options)
        {
            // ApplicationKey is needed to fetch user infos.
            options.ApplicationKey = _configuration.GetValue<string>("Steam:ApiKey");

            options.Events.OnAuthenticated = async ctx =>
            {
                var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();
                var mapper = ctx.HttpContext.RequestServices.GetRequiredService<IMapper>();
                var tokenIssuer = ctx.HttpContext.RequestServices.GetRequiredService<ITokenIssuer>();

                var player = ctx.UserPayload.RootElement
                    .GetProperty(SteamAuthenticationConstants.Parameters.Response)
                    .GetProperty(SteamAuthenticationConstants.Parameters.Players)[0]
                    .ToObject<SteamPlayer>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var user = await mediator.Send(mapper.Map<UpsertUserCommand>(player));

                var jwt = tokenIssuer.IssueToken(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, StringHelper.PascalToCamelCase(user.Role.ToString())),
                }));

                ctx.Request.HttpContext.Items["jwt"] = jwt;
            };

            options.Events.OnTicketReceived = ctx =>
            {
                ctx.HandleResponse();

                var jwt = ctx.Request.HttpContext.Items["jwt"] as string;
                ctx.Response.Redirect(QueryHelpers.AddQueryString(ctx.ReturnUri, "token", jwt));
                return Task.CompletedTask;
            };
        }
    }
}
