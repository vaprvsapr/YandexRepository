var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            var apiResult = new EventManager.Models.ApiResult
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = string.Join("; ", errors),
                DateTime = DateTime.Now
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(apiResult);
        };
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
