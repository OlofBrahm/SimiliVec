# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

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

# Create MLModels directory structure
RUN mkdir -p ./MLModels/e5-small-v2

# Copy tokenizer files (small files, not LFS)
COPY VectorDataBase/MLModels/e5-small-v2/tokenizer.json ./MLModels/e5-small-v2/
COPY VectorDataBase/MLModels/e5-small-v2/tokenizer_config.json ./MLModels/e5-small-v2/
COPY VectorDataBase/MLModels/e5-small-v2/vocab.txt ./MLModels/e5-small-v2/

# Download the large ONNX model from HuggingFace (bypass Git LFS issue)
RUN apt-get update && apt-get install -y curl && \
    curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/onnx/model.onnx" \
    -o ./MLModels/e5-small-v2/model.onnx && \
    apt-get remove -y curl && apt-get autoremove -y && rm -rf /var/lib/apt/lists/*

# Program.cs will read PORT env var at runtime
# No need to set ASPNETCORE_URLS here

ENTRYPOINT ["dotnet", "SimiliVec.Api.dll"]