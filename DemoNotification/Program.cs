using DemoNotification;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<UserInfoInMemory>();

builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigins",
        builder =>
        {
            builder
                .AllowCredentials()
                .AllowAnyHeader()
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .WithOrigins("https://localhost:7271");
        });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidIssuer = "https://localhost:7271",
    ValidAudience = "dataEventRecords",
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dataEventRecordsSecret")),
    NameClaimType = "name",
    RoleClaimType = "role",
};

var jwtSecurityTokenHandler = new JwtSecurityTokenHandler
{
    InboundClaimTypeMap = new Dictionary<string, string>()
};

builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = "https://localhost:7271";
    options.Audience = "dataEventRecords";
    options.IncludeErrorDetails = true;
    options.SaveToken = true;
    options.SecurityTokenValidators.Clear();
    options.SecurityTokenValidators.Add(jwtSecurityTokenHandler);
    options.TokenValidationParameters = tokenValidationParameters;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["token"];
            if (context.Request.Path.Value.StartsWith("/NotificationHubs")
                && !string.IsNullOrEmpty(token)
            )
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var te = context.Exception;
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddSwaggerGen();
var guestPolicy = new AuthorizationPolicyBuilder()
              .RequireClaim("scope", "dataEventRecords")
              .Build();
builder.Services.AddSignalR();
builder.Services.AddMvc();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowMyOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHubs>("/NotificationHubs");

app.MapControllers();

app.Run();
