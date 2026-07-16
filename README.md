# ApiAuth

API .NET para autenticação, gerenciamento de perfil e gerenciamento de treinos simples.

**Resumo:**
- **Propósito:** Serviço backend com JWT + refresh tokens, CRUD de perfil e endpoints para gerenciar uma ficha de treinos (exercícios por divisão A/B/C).
- **Público-alvo:** Frontends SPA (React/Vite, CRA) que necessitam de autenticação segura e refresh token.

**Tech stack:**
- .NET 10 (TargetFramework: `net10.0`)
- Entity Framework Core com `Npgsql` (PostgreSQL)
- JWT usando `Microsoft.AspNetCore.Authentication.JwtBearer`
- `BCrypt.Net-Next` para hashing de senhas
- `DotNetEnv` para carregar variáveis de ambiente durante o desenvolvimento
- OpenAPI/Scalar para documentação

**Arquivos importantes:**
- Program: [Program.cs](Program.cs)
- Configuração: [appsettings.json](appsettings.json)
- Serviços: [Services/TokenService.cs](Services/TokenService.cs), [Services/AuthService.cs](Services/AuthService.cs)
- Controllers: [Controllers/AuthController.cs](Controllers/AuthController.cs), [Controllers/ProfileController.cs](Controllers/ProfileController.cs), [Controllers/TreinoController.cs](Controllers/TreinoController.cs)
- DbContext: [Data/AppDbContext.cs](Data/AppDbContext.cs)

**Visão da API (endpoints principais)**
- `GET  /api/home` — Status básico
- `POST /api/auth/register` — Cadastro de usuário
  - Payload de exemplo:
  ```json
  {
    "nome": "João Silva",
    "email": "joao@example.com",
    "senha": "minhaSenha123"
  }
  ```
- `POST /api/auth/login` — Login
  - Payload de exemplo:
  ```json
  {
    "email": "joao@example.com",
    "senha": "minhaSenha123"
  }
  ```
  - Retorna objeto com `token` (JWT). O `AuthService` atualmente retorna apenas o access token no login.
- `POST /api/auth/refresh` — Troca de refresh token por novos tokens
  - Payload de exemplo:
  ```json
  {
    "refreshToken": "<string_refresh_token>"
  }
  ```
  - Retorno: `{ accessToken, refreshToken }` (quando válido)
- `POST /api/auth/logout` — Revoga um refresh token
  - Payload igual ao de `refresh`.
- `GET /api/profile` — Retorna perfil do usuário autenticado (Bearer JWT requerido)
- `PUT /api/profile` — Atualiza perfil do usuário autenticado
  - Payload: `UpdateProfileDto` (nome, email)
- `DELETE /api/profile` — Exclui conta do usuário autenticado
- `GET /api/treinos` — Retorna ficha de treinos agrupada por divisão (A, B, C)
- `POST /api/treinos` — Adiciona exercício à ficha (Bearer JWT)
  - Payload de exemplo:
  ```json
  {
    "exercicioId": 123,
    "divisao": "A",
    "nomeExercicio": "Supino Reto",
    "series": 4,
    "repeticoes": "8-10",
    "carga": "60kg",
    "descanso": "90s"
  }
  ```
- `DELETE /api/treinos/{id}` — Remove exercício (valida pertencimento ao usuário)

**Modelos principais (resumo):**
- `Usuario` — `Id`, `Nome`, `Email`, `SenhaHash`, `CriadoEm`, `RefreshTokens`
- `RefreshToken` — `Id`, `Token`, `ExpiraEm`, `Revogado`, `UsuarioId`
- `Treino` — `Id`, `UsuarioId`, `ExercicioId`, `Divisao`, `NomeExercicio`, `Series`, `Repeticoes`, `Carga`, `Descanso`
- DTOs: `LoginDto`, `RegisterDto`, `RefreshTokenDto`, `ProfileResponseDto`, `UpdateProfileDto`, `TreinoCriarDto`, `TreinoResponseDto` (pasta `DTOS/`)

**Autenticação e fluxo de refresh tokens (resumido):**
- Ao fazer login (`AuthService.LoginAsync`) a aplicação valida credenciais e gera um JWT via `TokenService.GerarToken(usuario)`.
- Refresh tokens são criados por `TokenService.GerarRefreshToken()` (valor aleatório base64) e persistidos em `RefreshTokens` via `RefreshTokenRepository`.
- `AuthService.RefreshTokenAsync` valida se o refresh existe, não expirou e não está revogado. Se válido, revoga o atual e persiste um novo refresh token além de retornar um novo access token.
- `Logout` apenas marca o refresh token como revogado.

**Configuração / Variáveis de ambiente**
- O projeto usa `DotNetEnv` para carregar `.env` localmente e também lê `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` e `CONNECTION_STRING` via variáveis de ambiente. As chaves podem ser fornecidas via `appsettings.json` durante desenvolvimento, mas em produção utilize variáveis de ambiente (Docker/Kubernetes/Cloud).
- Valores esperados em `appsettings.json` (ou env):
  - `Jwt:Key` — string secreta (mínimo 32 caracteres recomendado)
  - `Jwt:Issuer`, `Jwt:Audience`
  - `CONNECTION_STRING` ou `ConnectionStrings:DefaultConnection` — string de conexão PostgreSQL

**Como rodar localmente (exemplo)**
1. Configure as variáveis de ambiente ou crie um arquivo `.env` com:
```ini
CONNECTION_STRING=Host=localhost;Database=api_auth;Username=postgres;Password=postgres
Jwt__Key=uma_chave_super_secreta_com_32_chars_min
Jwt__Issuer=api-auth
Jwt__Audience=api-auth-client
Jwt__ExpiresInMinutes=60
```
2. Restaurar pacotes e rodar migrações (CLI .NET):
```bash
dotnet restore
dotnet ef database update
dotnet run
```
3. A API estará disponível em `https://localhost:5001` por padrão (dependendo do seu ambiente). Use a UI de OpenAPI/Scalar em desenvolvimento (Program.cs registra `MapOpenApi()` / `MapScalarApiReference`).

**Observações e recomendações (nível sênior):**
- Segurança:
  - Garanta que `Jwt:Key` nunca seja versionada. Use providers de segredo (Azure Key Vault, AWS Secrets Manager) em produção.
  - Adote `ILogger` estruturado (Serilog) para substituir os `Console.WriteLine` no `JwtBearerEvents.OnAuthenticationFailed`.
  - Considere usar `RefreshToken` com `Device`/`UserAgent` e `IpAddress` para proteção adicional e detecção de abuso.
- Banco de dados:
  - As migrations já estão presentes na pasta `Migrations/` — revise antes de rodar em produção.
  - Adicione índices conforme necessidade (já existe índice único em `RefreshToken.Token`).
- Escalabilidade:
  - O modelo atual persistente de refresh tokens é simples e funciona bem; para múltiplas instâncias, mantenha o banco como fonte central ou use cache distribuído.
- Melhorias possíveis:
  - Retornar no `login` tanto `accessToken` quanto `refreshToken` para simplificar o cliente.
  - Implementar expiração/rotacionamento de refresh tokens com histórico e blacklist mais robusta.

**Links úteis no projeto:**
- [Program.cs](Program.cs)
- [Services/TokenService.cs](Services/TokenService.cs)
- [Services/AuthService.cs](Services/AuthService.cs)
- [Controllers/AuthController.cs](Controllers/AuthController.cs)
- [Data/AppDbContext.cs](Data/AppDbContext.cs)

Se quiser, eu:
- Ajusto o `AuthService.LoginAsync` para retornar também o `refreshToken` (cliente espera ambos),
- Adiciono exemplos de chamadas `curl` ou um collection Postman/HTTP file (`ApiAuth.http` já presente),
- Crio testes unitários para os serviços críticos.

---
Gerado automaticamente — posso adaptar o README para o formato desejado (mais enxuto, com seção de deployment, ou com exemplos curl).