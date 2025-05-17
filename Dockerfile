FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build

# Set the working directory to /app and copy the entire repository.
WORKDIR /app
COPY . ./

# Restore NuGet packages.
RUN dotnet restore

# Define build-time variables (defaulting to Debug mode).
ARG BUILD_CONFIG=Debug
ARG PUBLISH_DIR=debug

RUN dotnet publish -c ${BUILD_CONFIG} -o ${PUBLISH_DIR}

FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff AS runtime
WORKDIR /app
COPY --from=build /app/${PUBLISH_DIR} .

ENTRYPOINT ["dotnet", "Infrastructure.dll"]
