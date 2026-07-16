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

// ApiAuth - Inicialização
// Este arquivo configura os serviços essenciais da aplicação:
// - Carregamento de variáveis de ambiente (.env em dev)
// - Configuração do EF/Core (Postgres)
// - Autenticação JWT (validação de issuer/audience/key)
// - Registrador de serviços e repositórios
// Observação: mantenha a ordem dos middlewares (CORS -> Auth -> Authorization).

DotNetEnv.Env.Load();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // Porta padrão do Vite (se usar Vite)
                "http://localhost:3000", // Porta padrão do Create React App
                "https://seu-app-react.vercel.app" // URL do seu React em produção (quando hospedar)
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Importante caso envie cookies ou headers de autenticação específicos
    });
});

builder.Services.AddControllers();

// Ler variáveis de ambiente (útil em containers/CI). A configuração pode vir
// tanto de `appsettings.json` quanto do ambiente (ex: Jwt__Key em Docker).
builder.Configuration.AddEnvironmentVariables();

// String de conexão lida via configuração/ambiente
var connectionString = builder.Configuration["CONNECTION_STRING"];

#endregion

#region 2. INJEÇÃO DE DEPENDÊNCIAS (IoC)

builder.Services.AddControllers();

// Serviços de Infraestrutura e Core da Aplicação
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ITreinoRepository, TreinoRepository>();
builder.Services.AddScoped<ITreinoService, TreinoService>();

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

// CORS: Permite que o front-end (React) faça requisições para esta API sem bloqueios de origem cruzada.
// CORS Sempre acima de Authentication/Authorization para que o navegador possa enviar os headers corretos.
app.UseCors("AllowReactApp");

// Ordem crítica de segurança: Quem você é (Authentication) deve vir ANTES do que você pode fazer (Authorization).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();