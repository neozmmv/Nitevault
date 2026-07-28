using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddSingleton<RedisService>(); // redis service
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString)); // uses db context
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

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment()) 
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
