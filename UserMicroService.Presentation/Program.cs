using Common.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text;
using UserMicroService.Infrastructure;
using UserMicroService.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUserInfrastructure(builder.Configuration);

// Добавление сервисов для логирования, мониторинга и трейсинга
builder.Services.AddObservability(builder.Configuration);
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console(new CompactJsonFormatter()));

builder.Services.AddControllers();

// Добавление сервисов для визуализации и документации API только в режиме разработки
if (builder.Environment.IsDevelopment())
    builder.Services.AddUserVisualization();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ??
                throw new SecurityTokenEncryptionKeyNotFoundException())
                ),

            RoleClaimType = "role",
            NameClaimType = "login"
        };

        options.MapInboundClaims = false;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

// Конфигурация middleware для визуализации и документации API только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPrometheusScrapingEndpoint(); // доступен по /metrics
// Пайплайн обработки запросов, включая глобальный обработчик исключений
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
// Для нормальной работы в контейнере Docker, где может не быть HTTPS, условно отключаем перенаправление на HTTPS в режиме разработки
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.MapControllers();
app.Run();