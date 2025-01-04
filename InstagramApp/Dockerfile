FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InstagramApp/InstagramApp.csproj", "InstagramApp/"]
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "InstagramApp/InstagramApp.csproj"
COPY . .
WORKDIR "/src/InstagramApp"
RUN dotnet build "InstagramApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InstagramApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InstagramApp.dll"]
