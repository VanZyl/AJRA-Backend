FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["AJRAApis/AJRAApis.csproj", "AJRAApis/"]
RUN dotnet restore "AJRAApis/AJRAApis.csproj"
COPY . .
WORKDIR "/src/AJRAApis"
RUN dotnet build "AJRAApis.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AJRAApis.csproj" -c $configuration -o /app/publish /p:UseAppHost=false --os linux --arch x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AJRAApis.dll"]
