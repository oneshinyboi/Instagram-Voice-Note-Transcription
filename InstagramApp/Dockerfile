FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
ARG DEBIAN_FRONTEND=noninteractive

# Install Chrome and Chromium dependencies
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    software-properties-common \
    && wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" > /etc/apt/sources.list.d/google-chrome.list' \
    && apt-get update && apt-get install -y \
    google-chrome-stable \
    chromium-browser \
    && rm -rf /var/lib/apt/lists/*

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

ENTRYPOINT ["dotnet", "InstagramApp.dll"]
