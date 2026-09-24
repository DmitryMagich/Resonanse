using Microsoft.EntityFrameworkCore;
using Resonanse.Infrastructure;
using Resonanse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext
builder.Services.AddDbContext<ResonanseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ResonanseDb")));

// Add services to the container.
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();