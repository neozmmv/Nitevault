using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

string? envDir = Directory.GetCurrentDirectory();
while (envDir != null && !File.Exists(Path.Combine(envDir, ".env")))
{
    envDir = Directory.GetParent(envDir)?.FullName;
}

if (envDir != null)
{
    DotNetEnv.Env.Load(Path.Combine(envDir, ".env"));
}

var builder = WebApplication.CreateBuilder(args);

// Redis
string redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";

// JWT KEY FROM ENV
string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY MUST BE SET IN .env!");

// POSTGRES URL
string dbConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? throw new InvalidOperationException("DATABASE_URL MUST BE SET IN .env!");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi 
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtService>(); // allow JwtService to be injected via DI
builder.Services.AddScoped<UserService>(); // allow UserServices to be injected via DI
builder.Services.AddScoped<AuthService>(); // AuthService via DI
builder.Services.AddScoped<StorageService>(); // Storage Service DI
builder.Services.AddSingleton<RedisService>(); // redis service
builder.Services.AddSingleton<DownloadTokenService>(); // download token service
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection)); // redis
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString)); // uses db context

// FOR DOCKERIZED IMAGE OF THIS PROJECT, USE CONTAINER NAME INSTEAD OF LOCALHOST.
// localhost -> telegram-bot-api
builder.Services.AddHttpClient<StorageService>(client =>
{
    client.BaseAddress = new Uri($"http://telegram-bot-api:8081/bot{Environment.GetEnvironmentVariable("BOT_TOKEN")}/");
    client.Timeout = TimeSpan.FromMinutes(10);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2_100_000_000;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "nitevault",
        ValidAudience = "nitevault-users",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents {
        OnMessageReceived = context =>
        {
            // authentication based on 'jwt' cookie instead of authorization header
            if(context.Request.Cookies.TryGetValue("jwt", out string? token) && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
   options.AddPolicy("AllowFrontend", policy =>
   {
      policy.SetIsOriginAllowed(origin =>
            {
                if (origin == "http://localhost:3000") return true;
                if (origin.EndsWith(".trycloudflare.com", StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); 
   });
});

builder.Services.Configure<FormOptions>(options =>
{
   options.MultipartBodyLengthLimit = 2_100_000_000; 
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply pending migrations on startup (creates the schema on first run)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment()) 
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
