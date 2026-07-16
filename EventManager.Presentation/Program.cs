using EventManager.Infrastructure.DataAccess;
using EventManager.Infrastructure.DI;
using EventManager.Presentation.DI;
using EventManager.Presentation.ExceptionHandling;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Добавление сервисов для визуализации и документации API только в режиме разработки
if (builder.Environment.IsDevelopment())
    builder.Services.AddVisualization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Конфигурация middleware для визуализации и документации API только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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