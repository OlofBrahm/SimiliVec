# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV PORT=8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["SimiliVec.Api/SimiliVec.Api.csproj", "SimiliVec.Api/"]
COPY ["VectorDataBase/VectorDataBase.csproj", "VectorDataBase/"]

# Restore dependencies
RUN dotnet restore "SimiliVec.Api/SimiliVec.Api.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/SimiliVec.Api"
RUN dotnet build "SimiliVec.Api.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "SimiliVec.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy ML models (IMPORTANT - include the models!)
COPY VectorDataBase/MLModels/ ./MLModels/

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "SimiliVec.Api.dll"]