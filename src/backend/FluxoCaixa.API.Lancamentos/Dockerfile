FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FluxoCaixa.API.Lancamentos/FluxoCaixa.API.Lancamentos.csproj", "FluxoCaixa.API.Lancamentos/"]
COPY ["FluxoCaixa.IOC/FluxoCaixa.IOC.csproj", "FluxoCaixa.IOC/"]
COPY ["FluxoCaixa.Infrastructure/FluxoCaixa.Infrastructure.csproj", "FluxoCaixa.Infrastructure/"]
COPY ["FluxoCaixa.Application/FluxoCaixa.Application.csproj", "FluxoCaixa.Application/"]
COPY ["FluxoCaixa.Domain/FluxoCaixa.Domain.csproj", "FluxoCaixa.Domain/"]
RUN dotnet restore "FluxoCaixa.API.Lancamentos/FluxoCaixa.API.Lancamentos.csproj"
COPY . .
WORKDIR "/src/FluxoCaixa.API.Lancamentos"
RUN dotnet build "FluxoCaixa.API.Lancamentos.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.API.Lancamentos.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluxoCaixa.API.Lancamentos.dll"]
