FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["src/ProductService/ProductService.Api/ProductService.Api.csproj", "src/ProductService/ProductService.Api/"]
COPY ["src/ProductService/ProductService.Application/ProductService.Application.csproj", "src/ProductService/ProductService.Application/"]
COPY ["src/ProductService/ProductService.Domain/ProductService.Domain.csproj", "src/ProductService/ProductService.Domain/"]
COPY ["src/ProductService/ProductService.Persistence/ProductService.Persistence.csproj", "src/ProductService/ProductService.Persistence/"]
COPY ["src/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "src/ProductService/ProductService.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/"]

RUN dotnet restore "src/ProductService/ProductService.Api/ProductService.Api.csproj" --disable-parallel

# Copy full source and build
COPY . .
WORKDIR "/src/src/ProductService/ProductService.Api"
RUN dotnet build "ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
