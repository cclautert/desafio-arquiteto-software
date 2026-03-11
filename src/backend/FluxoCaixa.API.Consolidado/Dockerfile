FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FluxoCaixa.API.Consolidado/FluxoCaixa.API.Consolidado.csproj", "FluxoCaixa.API.Consolidado/"]
COPY ["FluxoCaixa.IOC/FluxoCaixa.IOC.csproj", "FluxoCaixa.IOC/"]
COPY ["FluxoCaixa.Infrastructure/FluxoCaixa.Infrastructure.csproj", "FluxoCaixa.Infrastructure/"]
COPY ["FluxoCaixa.Application/FluxoCaixa.Application.csproj", "FluxoCaixa.Application/"]
COPY ["FluxoCaixa.Domain/FluxoCaixa.Domain.csproj", "FluxoCaixa.Domain/"]
RUN dotnet restore "FluxoCaixa.API.Consolidado/FluxoCaixa.API.Consolidado.csproj"
COPY . .
WORKDIR "/src/FluxoCaixa.API.Consolidado"
RUN dotnet build "FluxoCaixa.API.Consolidado.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.API.Consolidado.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluxoCaixa.API.Consolidado.dll"]
