using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNet.Security.OpenId;
using AspNet.Security.OpenId.Steam;
using AutoMapper;
using Crpg.Application;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Steam;
using Crpg.Application.System.Commands;
using Crpg.Application.Users.Commands;
using Crpg.Application.Users.Models;
using Crpg.Common.Helpers;
using Crpg.Common.Json;
using Crpg.Domain.Entities.Users;
using Crpg.Logging;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.WebApi.Identity;
using Crpg.WebApi.Services;
using Crpg.WebApi.Workers;
using IdentityServer4;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;
using LoggerFactory = Crpg.Logging.LoggerFactory;

var builder = WebApplication.CreateBuilder(args);
LoggerFactory.Initialize(builder.Configuration);
builder.Host.UseLogging();

var appEnv = ApplicationEnvironmentProvider.FromEnvironment();

builder.Services
    .AddSdk(builder.Configuration, appEnv)
    .AddPersistence(builder.Configuration, appEnv)
    .AddApplication()
    .AddHostedService<StrategusWorker>()
    .AddHttpContextAccessor() // Injects IHttpContextAccessor
    .AddScoped<ICurrentUserService, CurrentUserService>()
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(ConfigureSwagger)
    .AddCors(opts => ConfigureCors(opts, builder.Configuration))
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory(GeometryFactory.Default));
        options.JsonSerializerOptions.Converters.Add(new JsonArrayStringEnumFlagsConverterFactory());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddHealthChecks();

builder.Services.AddIdentity<UserViewModel, IdentityRole>()
    .AddRoleStore<NullRoleStore>()
    .AddUserStore<CustomUserStore>();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<UserViewModel>()
    .AddProfileService<CustomProfileService>()
    .AddInMemoryClients(builder.Configuration.GetSection("IdentityServer:Clients"))
    .AddInMemoryPersistedGrants()
    .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
    .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
    // DeveloperSigningCredential drawback is that it never get rotated but since new deployment recreate
    // all files this is fine.
    .AddDeveloperSigningCredential(filename: Path.Combine(Directory.GetCurrentDirectory(), "crpg.jwk"));

builder.Services.AddAuthentication()
    .AddJwtBearer(opts => ConfigureJwtBearer(opts, builder.Configuration))
    .AddSteam(opts => ConfigureSteamAuthentication(opts, builder.Configuration));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("User", BuildRolePolicy(Role.User, Role.Moderator, Role.Admin));
    options.AddPolicy("Moderator", BuildRolePolicy(Role.Moderator, Role.Admin));
    options.AddPolicy("Admin", BuildRolePolicy(Role.Admin));
    options.AddPolicy("Game", BuildScopePolicy("game_api"));
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else if (app.Environment.IsProduction())
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

ILogger logger = LoggerFactory.CreateLogger("Program");
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    var mediator = services.GetRequiredService<IMediator>();
    var db = services.GetRequiredService<CrpgDbContext>();

    string? skipMigrationStr = Environment.GetEnvironmentVariable("CRPG_SKIP_DB_MIGRATION");
    bool skipMigration = skipMigrationStr != null && bool.Parse(skipMigrationStr);
    bool hasConnectionString = app.Configuration.GetConnectionString("Crpg") != null;
    if (hasConnectionString && !skipMigration)
    {
        try
        {
            await db.Database.MigrateAsync();
            await using var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            conn.Open();
            conn.ReloadTypes();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while migrating the database.");
            LoggerFactory.Close();
            return 1;
        }
    }

    var res = await mediator.Send(new SeedDataCommand(), CancellationToken.None);
    if (res.Errors != null)
    {
        LoggerFactory.Close();
        return 1;
    }

    logger.LogInformation("cRPG Web API has started");
}

try
{
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    LoggerFactory.Close();
}

static AuthorizationPolicy BuildRolePolicy(params Role[] roles) =>
    new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("scope", "user_api")
        .RequireRole(roles.Select(r => r.ToString()))
        .Build();

static AuthorizationPolicy BuildScopePolicy(string scope) =>
    new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("scope", scope)
        .Build();

static void ConfigureCors(CorsOptions options, IConfiguration configuration)
{
    List<Client> clients = new();
    configuration.GetSection("IdentityServer:Clients").Bind(clients);

    // Get allowed origins from clients' redirect uris.
    string[] allowedOrigins = clients
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

static void ConfigureSwagger(SwaggerGenOptions options)
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

static void ConfigureJwtBearer(JwtBearerOptions options, IConfiguration configuration)
{
    options.Authority = configuration["IdentityServer:Authority"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // If the audience claim is not used, the audience check can be turned off
        ValidateAudience = false,
        ValidTypes = new[] { "at+jwt" },
    };
}

static void ConfigureSteamAuthentication(SteamAuthenticationOptions options, IConfiguration configuration)
{
    options.ApplicationKey = configuration["IdentityServer:Providers:Steam:ApplicationKey"];
    options.Events.OnAuthenticated = OnSteamUserAuthenticated;
}

static async Task OnSteamUserAuthenticated(OpenIdAuthenticatedContext ctx)
{
    var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();
    var mapper = ctx.HttpContext.RequestServices.GetRequiredService<IMapper>();
    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

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
