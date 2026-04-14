using Microsoft.EntityFrameworkCore;
using PhoneNumberApi.Data;
using PhoneNumberApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PhoneValidationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PhoneNumber API",
        Version = "v1",
        Description = "API для проверки и хранения телефонных номеров"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "PhoneNumberAPI",
    version = "1.0.0",
    status = "Running",
    endpoints = new
    {
        checkNumber = "POST /PhoneNumber/check_number",
        swagger = "GET /swagger (if Development)"
    }
}));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();