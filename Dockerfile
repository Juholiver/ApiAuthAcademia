# Estágio 1: Build da aplicação usando o SDK do .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o arquivo .csproj da API e restaura as dependências do NuGet
COPY ["ApiAuth/ApiAuth.csproj", "ApiAuth/"]
RUN dotnet restore "ApiAuth/ApiAuth.csproj"

# Copia todos os arquivos do repositório para o container
COPY . .
WORKDIR "/src/ApiAuth"

# Compila o projeto no modo Release sem o executável do host
RUN dotnet publish "ApiAuth.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Criação da imagem final leve de execução (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define a variável de ambiente para expor a porta que configuramos (8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiAuth.dll"]