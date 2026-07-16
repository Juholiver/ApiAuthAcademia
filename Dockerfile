# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o .csproj que está na raiz e restaura as dependências
COPY ["ApiAuth.csproj", "./"]
RUN dotnet restore "ApiAuth.csproj"

# Copia todos os arquivos restantes da raiz para o container
COPY . .

# Compila o projeto
RUN dotnet publish "ApiAuth.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiAuth.dll"]