using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OpenId.Steam;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Trpg.Application;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Steam;
using Trpg.Application.Users.Commands;
using Trpg.Infrastructure;
using Trpg.Persistence;
using Trpg.Web.Authentication;
using Trpg.Web.Common;
using Trpg.Web.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Trpg.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private IServiceCollection _services;

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
                .AddCors()
                .AddControllers();

            services.AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = NopAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddNop()
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

            app
                .UseCustomExceptionHandler()
                .UsePathBase("/api")
                .UseCors(options => options.AllowAnyOrigin())
                .UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trpg API"))
                .UseRouting()
                .UseAuthentication() // populate HttpContext.User
                .UseAuthorization() // check that HttpContext.User has the correct rights to access the endpoint
                .UseEndpoints(endpoints => endpoints.MapControllers());
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
                foreach (var svc in _services.OrderBy(s => s.ServiceType.FullName))
                    sb.Append("<tr>")
                        .Append($"<td>{svc.ServiceType.FullName}</td>")
                        .Append($"<td>{svc.Lifetime}</td>")
                        .Append($"<td>{svc.ImplementationType?.FullName}</td>")
                        .Append("</tr>");

                sb.Append("</tbody></table>");
                await context.Response.WriteAsync(sb.ToString());
            }));
        }

        private void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo {Title = "Trpg API", Version = "v1"});
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
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
                ValidateLifetime = true
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    if (ctx.Request.Cookies.TryGetValue("jwt", out var jwt))
                    {
                        ctx.Request.Headers[HeaderNames.Authorization] = "Bearer " + jwt;
                        ctx.Response.Cookies.Delete("jwt");
                    }

                    return Task.CompletedTask;
                }
            };
        }

        private void ConfigureSteamAuthentication(SteamAuthenticationOptions options)
        {
            // ApplicationKey is needed to fetch user infos.
            options.ApplicationKey = _configuration.GetValue<string>("Steam:ApiKey");
            options.CallbackPath = "/api/users/callback";
            options.Events.OnAuthenticated = async ctx =>
            {
                var mediator = ctx.HttpContext.RequestServices.GetRequiredService<IMediator>();
                var mapper = ctx.HttpContext.RequestServices.GetRequiredService<IMapper>();
                var tokenIssuer = ctx.HttpContext.RequestServices.GetRequiredService<ITokenIssuer>();

                var res = ctx.User[SteamAuthenticationConstants.Parameters.Response];
                var players = res[SteamAuthenticationConstants.Parameters.Players];
                var player = players.First.ToObject<SteamPlayer>();

                var user = await mediator.Send(mapper.Map<UpsertUserCommand>(player));

                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                });
                var jwt = tokenIssuer.IssueToken(claimsIdentity);
                ctx.Response.Cookies.Append("jwt", jwt, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = ctx.Request.IsHttps,
                    IsEssential = true
                });
            };
        }
    }
}