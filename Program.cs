using ApiAuth.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Adiciona o gerador de OpenAPI do .NET 9
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 1. Gera o endpoint do JSON da especificação (por padrão em /openapi/v1.json)
    app.MapOpenApi();
    
    // 2. Configura o Scalar para ler o documento gerado pelo MapOpenApi
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Minha API Auth")
               .WithTheme(ScalarTheme.DeepSpace) // Escolha um tema opcional
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();