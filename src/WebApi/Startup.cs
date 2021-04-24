using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNet.Security.OpenId;
using AspNet.Security.OpenId.Steam;
using AutoMapper;
using Crpg.Application;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Steam;
using Crpg.Application.Users.Commands;
using Crpg.Application.Users.Models;
using Crpg.Common.Helpers;
using Crpg.Common.Json;
using Crpg.Domain.Entities.Users;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.WebApi.Identity;
using Crpg.WebApi.Services;
using IdentityServer4;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Swashbuckle.AspNetCore.SwaggerGen;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Crpg.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appEnv = ApplicationEnvironmentProvider.FromEnvironment();

            services
                .AddSdk(_configuration, appEnv)
                .AddPersistence(_configuration, appEnv)
                .AddApplication()
                .AddHttpContextAccessor() // Injects IHttpContextAccessor
                .AddScoped<ICurrentUserService, CurrentUserService>()
                .AddSwaggerGen(ConfigureSwagger)
                .AddCors(ConfigureCors)
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
                    options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory(GeometryFactory.Default));
                    options.JsonSerializerOptions.Converters.Add(new JsonArrayStringEnumFlagsConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddHealthChecks();

            services.AddIdentity<UserViewModel, IdentityRole>()
                .AddRoleStore<NullRoleStore>()
                .AddUserStore<CustomUserStore>();

            services.AddIdentityServer()
                .AddAspNetIdentity<UserViewModel>()
                .AddProfileService<CustomProfileService>()
                .AddInMemoryClients(_configuration.GetSection("IdentityServer:Clients"))
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
                .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
                // DeveloperSigningCredential drawback is that it never get rotated but since new deployment recreate
                // all files this is fine.
                .AddDeveloperSigningCredential(filename: Path.Combine(Directory.GetCurrentDirectory(), "crpg.jwk"));

            services.AddAuthentication()
                .AddJwtBearer(ConfigureJwtBearer)
                .AddSteam(ConfigureSteamAuthentication);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("User", BuildRolePolicy(Role.User, Role.Moderator, Role.Admin));
                options.AddPolicy("Moderator", BuildRolePolicy(Role.Moderator, Role.Admin));
                options.AddPolicy("Admin", BuildRolePolicy(Role.Admin));
                options.AddPolicy("Game", BuildScopePolicy("game_api"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx#use-a-reverse-proxy-server
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                });
            }

            app
                .UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crpg API"))
                .UseRouting()
                .UseCors()
                .UseIdentityServer()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health");
                    endpoints.MapControllers();
                });
        }

        private static AuthorizationPolicy BuildRolePolicy(params Role[] roles) =>
            new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "user_api")
                .RequireRole(roles.Select(r => r.ToString()))
                .Build();

        private static AuthorizationPolicy BuildScopePolicy(string scope) =>
            new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim("scope", scope)
                .Build();

        private void ConfigureCors(CorsOptions options)
        {
            var clients = new List<Client>();
            _configuration.GetSection("IdentityServer:Clients").Bind(clients);

            // Get allowed origins from clients' redirect uris.
            var allowedOrigins = clients
                .SelectMany(c => c.RedirectUris)
                .Select(uri => new Uri(uri).GetLeftPart(UriPartial.Authority))
                .Distinct()
                .ToArray();

            options.AddDefaultPolicy(builder => builder
                .WithOrigins(allowedOrigins)
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
                            Id = "Bearer",
                        },
                    },
                    new List<string>()
                },
            });
        }

        private void ConfigureJwtBearer(JwtBearerOptions options)
        {
            options.Authority = _configuration["IdentityServer:Authority"];

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // If the audience claim is not used, the audience check can be turned off
                ValidateAudience = false,
                ValidTypes = new[] { "at+jwt" },
            };
        }

        private void ConfigureSteamAuthentication(SteamAuthenticationOptions options)
        {
            options.ApplicationKey = _configuration["IdentityServer:Providers:Steam:ApplicationKey"];
            options.Events!.OnAuthenticated = OnSteamUserAuthenticated;
        }

        private async Task OnSteamUserAuthenticated(OpenIdAuthenticatedContext ctx)
        {
            var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var mapper = ctx.HttpContext.RequestServices.GetRequiredService<IMapper>();
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();

            var player = ctx.UserPayload!.RootElement
                .GetProperty(SteamAuthenticationConstants.Parameters.Response)
                .GetProperty(SteamAuthenticationConstants.Parameters.Players)[0]
                .ToObject<SteamPlayer>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = await mediator.Send(mapper.Map<UpsertUserCommand>(player));
            await ctx.HttpContext.SignInAsync(new IdentityServerUser(result.Data!.Id.ToString()));

            // Delete temporary cookie used during external authentication
            await ctx.HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            logger.LogInformation("User '{0}' signed in", result.Data!.Id);
        }
    }
}
