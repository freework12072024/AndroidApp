using Api.Hubs;
using Api.Models;
using Api.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var firebaseJson = Environment.GetEnvironmentVariable("Firebase__ServiceAccountJson");

if (string.IsNullOrWhiteSpace(firebaseJson))
{
    var firebaseFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "firebasesdk.json");

    if (File.Exists(firebaseFile))
    {
        firebaseJson = File.ReadAllText(firebaseFile);
    }
}

Console.WriteLine(
    $"Firebase config loaded: {!string.IsNullOrWhiteSpace(firebaseJson)}");

Console.WriteLine(
    $"Firebase config length: {firebaseJson?.Length ?? 0}");
if (string.IsNullOrWhiteSpace(firebaseJson))
{
    throw new InvalidOperationException(
        "Firebase__ServiceAccountJson environment variable is required.");
}

FirebaseApp.Create(new AppOptions
{
    Credential = CredentialFactory
    .FromJson<ServiceAccountCredential>(firebaseJson)
    .ToGoogleCredential()
});

// Allow overriding connection string via environment in Docker
var conn = builder.Configuration.GetConnectionString("SQLConnectionString") ?? builder.Configuration["ConnectionStrings:SQLConnectionString"];
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__SQLConnectionString")))
{
    conn = Environment.GetEnvironmentVariable("ConnectionStrings__SQLConnectionString");
}

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(conn));


builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register the in-memory email queue and background worker
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<EmailSenderWorker>();
builder.Services.AddSingleton<
    IPushNotificationService,
    FirebasePushNotificationService>();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = key
    };
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];

            if (!string.IsNullOrWhiteSpace(token) &&
                context.HttpContext.Request.Path
                    .StartsWithSegments("/chatHub"))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Apply migrations in Development or when running in Docker
if (app.Environment.IsDevelopment() || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


    app.UseSwagger();
    app.UseSwaggerUI();


// Only use HTTPS redirection when not running inside a container (container binds HTTP only)
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    app.UseHttpsRedirection();
}

// Enable CORS before other middleware that handles requests
app.UseCors();

// Ensure authentication middleware is registered before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<ChatHub>("/chatHub");

app.Run();
