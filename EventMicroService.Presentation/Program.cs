using EventMicroService.Infrastructure;
using EventMicroService.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEventInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Добавление сервисов для визуализации и документации API только в режиме разработки
if (builder.Environment.IsDevelopment())
    builder.Services.AddEventVisualization();



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
    var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
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