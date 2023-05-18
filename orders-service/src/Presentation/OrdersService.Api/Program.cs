using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using OrdersService.Api.Extensions;
using OrdersService.Api.Options;
using OrdersService.Api.Services;
using OrdersService.Application;
using OrdersService.Application.Configuration.Extensions;
using OrdersService.Application.Services.Interfaces;
using OrdersService.Infrastructure.ElasticSearch.Configuration.Extensions;
using OrdersService.Infrastructure.EventStoreDB.Configuration.Extensions;
using OrdersService.Infrastructure.EventStoreDB.Subscriptions;

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

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = HeaderNames.Authorization,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new List<string>()
                }
            });
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
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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
    })
    .Services
    .AddHealthChecks()
    .Services
    .AddEasyNetQ(rabbitMqOptions)
    .AddHttpContextAccessor()
    .AddTransient<IUserAccessor, UserAccessor>()
    .AddApplication()
    .AddInfrastructureEventStoreDb()
    .AddInfrastructureElasticSearch();

builder.Services
    .AddTransient(serviceProvider => new MapperConfiguration(config =>
        {
            config.AddProfile<MapperProfile>();
            config.AddProfile<OrdersService.Infrastructure.ElasticSearch.MapperProfile>();
            config.AddProfile(new OrdersService.Api.MapperProfile(serviceProvider.GetRequiredService<IUserAccessor>()));
        })
        .CreateMapper())
    .AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

WebApplication app = builder.Build();

await app.Services
    .GetRequiredService<OrderSubscription>()
    .SubscribeToOrderUpdates();

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

//await app.Services.GetRequiredService<IElasticClient>().Indices.CreateAsync(
//    app.Services.GetRequiredService<IOptions<ElasticSearchOptions>>().Value.IndexName,
//    index => index.Map<Order>(config => config.AutoMap()));

app.MapControllers();
app.Run();

namespace OrdersService.Api
{
    public partial class Program // Is needed for WebApplicationFactory
    {
    }
}