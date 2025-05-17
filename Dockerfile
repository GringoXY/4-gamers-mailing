# See https://aka.ms/customizecontainer for more details.

# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

# Stage 2: Build image using the SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mailing/Mailing.csproj", "Mailing/"]
RUN dotnet restore "Mailing/Mailing.csproj"
COPY . .
WORKDIR "/src/Mailing"
RUN dotnet build "Mailing.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mailing.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mailing.dll"]
