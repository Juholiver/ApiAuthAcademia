using ApiAuth.Data;
using ApiAuth.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// 1. Carrega as variáveis do arquivo .env para o ambiente do sistema
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 2. CORREÇÃO CRÍTICA: Diz para o .NET ler as variáveis de ambiente do sistema.
builder.Configuration.AddEnvironmentVariables();

// 3. Agora você pode pegar a connection string direto do Configuration de forma limpa!
var connectionString = builder.Configuration["CONNECTION_STRING"];

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<TokenService>();

// CONFIGURAÇÃO DO OPENAPI/SCALAR
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // O .NET agora vai achar os valores reais do .env aqui:
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!))
        };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Minha API Auth")
               .WithTheme(ScalarTheme.DeepSpace) 
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();