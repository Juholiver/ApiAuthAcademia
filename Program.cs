using ApiAuth.Data;
using ApiAuth.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using ApiAuth.Interfaces;
using ApiAuth.Repositories;
using ApiAuth.Middlewares;

// 1. Carrega as variáveis do arquivo .env para o ambiente do sistema
DotNetEnv.Env.Load();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 2. CORREÇÃO CRÍTICA: Diz para o .NET ler as variáveis de ambiente do sistema.
builder.Configuration.AddEnvironmentVariables();

// 3. Agora você pode pegar a connection string direto do Configuration de forma limpa!
var connectionString = builder.Configuration["CONNECTION_STRING"];

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IProfileService, ProfileService>();

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
    // Recupera os valores de forma robusta igual feito no TokenService
    var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") 
                 ?? builder.Configuration["Jwt:Key"];
                 
    var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") 
                    ?? builder.Configuration["Jwt:Issuer"];
                    
    var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") 
                      ?? builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("A chave JWT não pôde ser carregada no Program.cs.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Mude para true se você gerou o token com Issuer
        ValidateAudience = true, // Mude para true se você gerou o token com Audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("\n--- FALHA NA AUTENTICAÇÃO JWT ---");
            Console.WriteLine($"Erro: {context.Exception.Message}");
            if (context.Exception.InnerException != null)
            {
                Console.WriteLine($"Inner Erro: {context.Exception.InnerException.Message}");
            }
            Console.WriteLine("---------------------------------\n");
            return Task.CompletedTask;
        }
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
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();