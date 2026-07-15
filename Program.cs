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

#region 1. CONFIGURAÇÕES INICIAIS E VARIÁVEIS DE AMBIENTE

// Carrega as variáveis do arquivo .env para o ambiente do sistema operacional.
// Útil para desenvolvimento local, simulando containers ou ambientes de nuvem.
DotNetEnv.Env.Load();

// Evita que o .NET mapeie os claims padrão do JWT (como 'sub' para NameIdentifier).
// Mantém as chaves exatamente como foram geradas no token (ex: "id", "email"), facilitando a leitura e compatibilidade.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Força o provedor de configuração do .NET a ler variáveis de ambiente do sistema.
// Essencial para produção (Docker, AWS, Azure), permitindo que 'Jwt__Key' seja interpretada corretamente como 'Jwt:Key'.
builder.Configuration.AddEnvironmentVariables();

// Recupera a string de conexão centralizada na configuração.
var connectionString = builder.Configuration["CONNECTION_STRING"];

#endregion

#region 2. INJEÇÃO DE DEPENDÊNCIAS (IoC)

builder.Services.AddControllers();

// Serviços de Infraestrutura e Core da Aplicação
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Documentação da API via OpenAPI (antigo Swagger) integrado ao Scalar
builder.Services.AddOpenApi();

// Configuração do Banco de Dados (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

#endregion

#region 3. CONFIGURAÇÃO DE SEGURANÇA E AUTENTICAÇÃO (JWT)

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Fallback defensivo: Tenta ler no padrão de ambiente Unix/Docker (__), 
        // caso contrário busca do Configuration padrão.
        var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") 
                     ?? builder.Configuration["Jwt:Key"];
                     
        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") 
                        ?? builder.Configuration["Jwt:Issuer"];
                        
        var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") 
                          ?? builder.Configuration["Jwt:Audience"];

        // Fail-Fast: Interrompe a inicialização da aplicação imediatamente se a chave secreta estiver ausente.
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("A chave JWT não pôde ser carregada no Program.cs. Verifique as variáveis de ambiente.");
        }

        // Regras estritas de validação do token recebido nas requisições
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidateAudience = true, 
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Interceptores de eventos do ciclo de vida da autenticação
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Logs de diagnóstico para falhas de autenticação. 
                // Dica Sênior: Em produção, direcione isso para um ILogger/Serilog estruturado em vez do Console.
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

#endregion

var app = builder.Build();

#region 4. PIPELINE DE MIDDLEWARES (HTTP REQUEST PIPELINE)

// Ambiente de Desenvolvimento: Ativa ferramentas de exploração e teste de endpoints
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Expõe o JSON do OpenAPI
    
    // Interface gráfica moderna para testes da API substituindo o Swagger UI tradicional
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Minha API Auth")
               .WithTheme(ScalarTheme.DeepSpace) 
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

// Middleware Global de Exceções: Captura erros não tratados e formata respostas HTTP limpas.
// Nota de ordem: Deve ficar antes de Auth para capturar falhas em todo o fluxo da requisição.
app.UseMiddleware<ExceptionMiddleware>();

// Ordem crítica de segurança: Quem você é (Authentication) deve vir ANTES do que você pode fazer (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();