FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/DJIMarket.Domain/DJIMarket.Domain.csproj src/DJIMarket.Domain/
COPY src/DJIMarket.Application/DJIMarket.Application.csproj src/DJIMarket.Application/
COPY src/DJIMarket.Infrastructure/DJIMarket.Infrastructure.csproj src/DJIMarket.Infrastructure/
COPY src/DJIMarket.Api/DJIMarket.Api.csproj src/DJIMarket.Api/
RUN dotnet restore src/DJIMarket.Api/DJIMarket.Api.csproj
COPY . .
RUN dotnet publish src/DJIMarket.Api/DJIMarket.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV DOTNET_EnableDiagnostics=0
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DJIMarket.Api.dll"]
