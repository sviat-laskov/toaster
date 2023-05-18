using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PushUpdatesHub.Api.Extensions;
using PushUpdatesHub.Api.Options;
using PushUpdatesHub.Api.Services;
using PushUpdatesHub.Api.Services.Interfaces;
using PushUpdatesHub.Common.Constants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var keycloakOptions = builder.Configuration.GetRequiredSection("Keycloak").Get<KeycloakOptions>();
var rabbitMqOptions = builder.Configuration.GetRequiredSection("RabbitMQ").Get<RabbitMqOptions>();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            options.SupportNonNullableReferenceTypes();
            options.DescribeAllParametersInCamelCase();
        })
        .Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        })
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });
}

builder.Services
    .AddCors()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakOptions.Uri.ToString();
        options.Audience = keycloakOptions.Audience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = keycloakOptions.Authority.ToString(),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireExpirationTime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.IsPushUpdateHubHandShake())
                {
                    context.SetAccessTokenFromQuery();
                }

                return Task.CompletedTask;
            }
        };
    }).Services
    .AddHealthChecks().Services
    .AddSignalR().Services
    .AddEasyNetQ(rabbitMqOptions)
    .AddSingleton<IPushUpdateService, PushUpdateService>();

WebApplication app = builder.Build();

await app.SubscribeToIntegrationMessages();

app.MapHealthChecks("/health");
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
    app
        .UseSwagger()
        .UseSwaggerUI();
}

app.UseCors(corsPolicyBuilder => corsPolicyBuilder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true));
app
    .UseAuthentication()
    .UseAuthorization();
app.MapHub<PushUpdatesHub.Api.Hubs.PushUpdatesHub>(PushUpdateConstants.HubPath);

app.Run();

namespace PushUpdatesHub.Api
{
    public partial class Program // Is needed for WebApplicationFactory
    {
    }
}