FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app

# Build Stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src

# Copy .csproj file and restore dependencies
COPY AJRAApis.csproj .
RUN dotnet restore "AJRAApis.csproj"

# Copy the rest of the app
COPY . .

# Build application
RUN dotnet build "AJRAApis.csproj" -c $configuration -o /app/build

# Publish application
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AJRAApis.csproj" -c $configuration -o /app/publish /p:UseAppHost=false --os linux --arch x64

# Final Stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AJRAApis.dll"]
