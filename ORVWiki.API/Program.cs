using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ORVWiki.API.Auth;
using ORVWiki.API.Middleware;
using ORVWiki.API.OpenApi;
using ORVWiki.API.Realtime;
using ORVWiki.Application;
using ORVWiki.Application.Auth;
using ORVWiki.Application.Notifications;
using ORVWiki.Infrastructure;
using ORVWiki.Infrastructure.Auth;
using ORVWiki.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog: read sinks/min levels from appsettings, enrich with request scope.
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Serialize enums as snake_case strings so the wire shape matches the
        // frontend's ENTITY_TYPES keys ("character", "demon_king", ...) and
        // makes string ops like .toLowerCase() on enum-valued fields safe.
        // Without this, ASP.NET emits enums as integers (0, 1, ...) and the
        // frontend's category lookup, search results, and timeline all break.
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.SnakeCaseLower));
    });
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationPusher, SignalRNotificationPusher>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Browsers can't set Authorization headers on WebSocket upgrades, so
        // accept the access token from the query string for /hubs/* paths.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Reflect any origin (incl. file:// which sends Origin: null) so local
        // dev frontends — opened from disk or any localhost port — can call us.
        p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }
    else
    {
        p.WithOrigins("https://claude.ai").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }
}));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Admin, p => p.RequireRole(Roles.Admin));
    options.AddPolicy(AuthPolicies.Editor, p => p.RequireRole(Roles.Editor, Roles.Admin));
    options.AddPolicy(AuthPolicies.Reader, p => p.RequireRole(Roles.Reader, Roles.Editor, Roles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await DbInitializer.InitializeAsync(app.Services);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
