using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNet.Security.OpenId;
using AspNet.Security.OpenId.Steam;
using Crpg.Application;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Steam;
using Crpg.Application.System.Commands;
using Crpg.Application.Users.Commands;
using Crpg.Common.Helpers;
using Crpg.Common.Json;
using Crpg.Domain.Entities.Users;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Crpg.WebApi.Identity;
using Crpg.WebApi.Services;
using Crpg.WebApi.Workers;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Npgsql;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using LoggerFactory = Crpg.Logging.LoggerFactory;

var appEnv = ApplicationEnvironmentProvider.FromEnvironment();

if (appEnv.Environment == HostingEnvironment.Development)
{
    IdentityModelEventSource.ShowPII = true;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSdk(builder.Configuration, appEnv)
    .AddPersistence(builder.Configuration, appEnv)
    .AddApplication(builder.Configuration, appEnv)
    // .AddHostedService<StrategusWorker>() Disable strategus for now.
    .AddHostedService<DonorSynchronizerWorker>()
    .AddHostedService<ActivityLogsCleanerWorker>()
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

builder.Services.AddDbContext<OpenIddictDbContext>(options =>
{
    // Use in-memory provider just because it's easier to setup. Though @k.chalet says:
    // > I wouldn't recommend it, as OpenIddict uses the authorizations and tokens
    // > tables to keep track of individual tokens and logical chains of tokens and
    // > revoke them if necessary (e.g if you reuse an authorization code or a refresh
    // > token after the leeway period, the entire chain is revoked). If you don't
    // > have a persistent storage, you'll eventually get errors indicating the backing
    // > entries cannot be found and the UX won't be great at all.
    options.UseInMemoryDatabase("openiddict");
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<OpenIddictDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetLogoutEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token");

        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow()
            .AllowClientCredentialsFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate()
            .DisableAccessTokenEncryption();

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough();
    })
    .AddClient(options =>
    {
        bool webProviderAdded = false;
        var webIntegrationBuilder = options.UseWebProviders();

        string? epicGamesClientId = builder.Configuration["EpicGames:ClientId"];
        string? epicGamesClientSecret = builder.Configuration["EpicGames:ClientSecret"];

        if (epicGamesClientId != null && epicGamesClientSecret != null)
        {
            webIntegrationBuilder.UseEpicGames(epicGames =>
            {
                epicGames
                    .SetClientId(epicGamesClientId)
                    .SetClientSecret(epicGamesClientSecret)
                    .SetRedirectUri("connect/callback-epic-games");
            });
            webProviderAdded = true;
        }

        if (!webProviderAdded)
        {
            return;
        }

        options.AllowAuthorizationCodeFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication()
    .AddCookie()
    .AddSteam(opts => ConfigureSteamAuthentication(opts, builder.Configuration));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("User", BuildRolePolicy(Role.User, Role.Moderator, Role.Admin));
    options.AddPolicy("Moderator", BuildRolePolicy(Role.Moderator, Role.Admin));
    options.AddPolicy("Admin", BuildRolePolicy(Role.Admin));
    options.AddPolicy("Game", BuildScopePolicy("game_api"));
});

var app = builder.Build();
// Get the ASP.NET Core logger and store in a static variable.
LoggerFactory.Initialize(app.Services.GetRequiredService<ILoggerFactory>());
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
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapHealthChecks("/health");
        endpoints.MapControllers();
    });

ILogger logger = LoggerFactory.CreateLogger<Program>();
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
            var conn = (NpgsqlConnection)db.Database.GetDbConnection(); // Don't dispose!
            conn.Open();
            conn.ReloadTypes();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while migrating the database.");
            LoggerFactory.Dispose();
            return 1;
        }
    }

    try
    {
        var clientManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        await UpdateIdentityServerConfigurationAsync(app.Configuration, clientManager, scopeManager);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred while updating identity server configuration");
        LoggerFactory.Dispose();
        return 1;
    }

    var res = await mediator.Send(new SeedDataCommand(), CancellationToken.None);
    if (res.Errors != null)
    {
        LoggerFactory.Dispose();
        return 1;
    }
}

await app.RunAsync();
return 0;

static AuthorizationPolicy BuildRolePolicy(params Role[] roles) =>
    new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireAssertion(c => c.User.HasScope("user_api"))
        .RequireRole(roles.Select(r => r.ToString()))
        .Build();

static AuthorizationPolicy BuildScopePolicy(string scope) =>
    new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireAssertion(c => c.User.HasScope(scope))
        .Build();

static void ConfigureCors(CorsOptions options, IConfiguration configuration)
{
    List<OpenIddictApplicationDescriptor> clients = new();
    configuration.GetSection("OpenIddict:Clients").Bind(clients);

    // Get allowed origins from clients' redirect uris.
    string[] allowedOrigins = clients
        .SelectMany(c => c.RedirectUris)
        .Select(uri => uri.GetLeftPart(UriPartial.Authority))
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

static void ConfigureSteamAuthentication(SteamAuthenticationOptions options, IConfiguration configuration)
{
    options.ApplicationKey = configuration["Steam:ApiKey"];
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Events.OnAuthenticated = OnSteamUserAuthenticated;
}

static async Task OnSteamUserAuthenticated(OpenIdAuthenticatedContext ctx)
{
    var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();

    if (ctx.UserPayload == null)
    {
        throw new InvalidOperationException("Steam API key was not set");
    }

    var player = ctx.UserPayload.RootElement
        .GetProperty(SteamAuthenticationConstants.Parameters.Response)
        .GetProperty(SteamAuthenticationConstants.Parameters.Players)[0]
        .ToObject<SteamPlayer>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    var res = await mediator.Send(new UpsertUserCommand
    {
        Platform = Platform.Steam,
        PlatformUserId = player.SteamId,
        Name = player.PersonaName,
        Avatar = player.AvatarFull,
    });

    ctx.Identity!.SetClaim(OpenIddictConstants.Claims.Subject, res.Data!.Id.ToString());
}

static async Task UpdateIdentityServerConfigurationAsync(
    IConfiguration configuration,
    IOpenIddictApplicationManager clientManager,
    IOpenIddictScopeManager scopeManager)
{
    List<OpenIddictApplicationDescriptor> clients = new();
    configuration.GetSection("OpenIddict:Clients").Bind(clients);

    foreach (var client in clients)
    {
        object? existingClient = await clientManager.FindByClientIdAsync(client.ClientId!);
        if (existingClient == null)
        {
            await clientManager.CreateAsync(client);
        }
        else
        {
            await clientManager.UpdateAsync(existingClient, client);
        }
    }

    List<OpenIddictScopeDescriptor> scopes = new();
    configuration.GetSection("OpenIddict:Scopes").Bind(scopes);

    foreach (var scope in scopes)
    {
        object? existingScope = await scopeManager.FindByIdAsync(scope.Name!);
        if (existingScope == null)
        {
            await scopeManager.CreateAsync(scope);
        }
        else
        {
            await scopeManager.UpdateAsync(existingScope, scope);
        }
    }
}
