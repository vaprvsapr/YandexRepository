using EventManager.Infrastructure.DataAccess;
using EventManager.Infrastructure.DI;
using EventManager.Presentation.DI;
using EventManager.Presentation.ExceptionHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Добавление сервисов для визуализации и документации API только в режиме разработки
if (builder.Environment.IsDevelopment())
    builder.Services.AddVisualization();



builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SekretKey"] ??
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
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Конфигурация middleware для визуализации и документации API только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Пайплайн обработки запросов, включая глобальный обработчик исключений
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();