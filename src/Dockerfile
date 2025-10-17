# Multi-stage Dockerfile for MediaBrowser
# Stage 1: Build the Angular frontend
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Copy package.json and package-lock.json (if available)
COPY MediaBrowser.Frontend/package*.json ./

# Install dependencies
RUN npm ci

# Copy the frontend source code
COPY MediaBrowser.Frontend/ ./

# Create the wwwroot directory in the MediaBrowser project
RUN mkdir -p ../MediaBrowser/wwwroot

# Build the frontend
RUN npm run build

# Stage 2: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS dotnet-build

WORKDIR /app

# Copy solution file and project files
COPY MediaBrowser.slnx ./
COPY MediaBrowser/MediaBrowser.csproj ./MediaBrowser/
COPY MediaBrowser.Common/MediaBrowser.Common.csproj ./MediaBrowser.Common/
COPY Migrations/MediaBrowser.MySql/MediaBrowser.MySql.csproj ./Migrations/MediaBrowser.MySql/
COPY Migrations/MediaBrowser.Postgres/MediaBrowser.Postgres.csproj ./Migrations/MediaBrowser.Postgres/
COPY Migrations/MediaBrowser.Sqlite/MediaBrowser.Sqlite.csproj ./Migrations/MediaBrowser.Sqlite/
COPY Migrations/MediaBrowser.SqlServer/MediaBrowser.SqlServer.csproj ./Migrations/MediaBrowser.SqlServer/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Copy the built frontend from the previous stage
COPY --from=frontend-build /app/MediaBrowser/wwwroot ./MediaBrowser/wwwroot/

# Build the application
RUN dotnet publish MediaBrowser/MediaBrowser.csproj -c Release -o /app/publish

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime

WORKDIR /app

# Create a non-root user
RUN useradd -r -s /bin/false appuser

# Copy the published application
COPY --from=dotnet-build /app/publish ./

# Change ownership to the non-root user
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Expose the port the app runs on
EXPOSE 5050

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "MediaBrowser.dll"]