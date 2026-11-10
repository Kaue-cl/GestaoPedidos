
# Etapa 1 — Compilação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copia o projeto e restaura as dependências
COPY GestaoPedidos.API/GestaoPedidos.API.csproj GestaoPedidos.API/

RUN dotnet restore GestaoPedidos.API/GestaoPedidos.API.csproj

# Copia os arquivos da aplicação
COPY GestaoPedidos.API/ GestaoPedidos.API/

# Publica a aplicação
RUN dotnet publish GestaoPedidos.API/GestaoPedidos.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 2 — Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Configura a API para HTTP na porta 8080
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Diretório do banco SQLite
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "GestaoPedidos.API.dll"]
