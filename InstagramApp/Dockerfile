FROM fedora:latest AS base
USER $APP_UID
WORKDIR /app

RUN dnf install -y dnf-plugins-core \
    && dnf install -y fedora-workstation-repositories \
    && sudo dnf config-manager --set-enabled google-chrome \
    && dnf install -y google-chrome-stable chromedriver \
    && dnf clean all

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InstagramApp/InstagramApp.csproj", "InstagramApp/"]
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "InstagramApp/InstagramApp.csproj"
COPY . .
WORKDIR "/src/InstagramApp"
RUN dotnet build "InstagramApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build-stage AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InstagramApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV PATH="/usr/bin:$PATH"
ENTRYPOINT ["dotnet", "InstagramApp.dll"]
