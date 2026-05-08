FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY SIFS.csproj ./
RUN dotnet restore ./SIFS.csproj
COPY . .
RUN dotnet publish ./SIFS.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /data
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SIFS.dll"]
