using EventManager.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddControllers();

// Добавление сервисов для визуализации и документации API только в режиме разработки
if (builder.Environment.IsDevelopment())
    builder.Services.AddVisualization();

var app = builder.Build();

// Конфигурация middleware для визуализации и документации API только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
