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
COPY ["UMAPuwotSharp/UMAPuwotSharp.csproj", "UMAPuwotSharp/"]

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

# Copy published app (should already include native libraries via .props file)
COPY --from=publish /app/publish .

# EXPLICITLY copy UMAP native libraries (failsafe)
COPY --from=publish /app/publish/runtimes ./runtimes

# Create MLModels directory structure
RUN mkdir -p ./MLModels/e5-small-v2

# Copy tokenizer files
COPY VectorDataBase/MLModels/e5-small-v2/tokenizer.json ./MLModels/e5-small-v2/
COPY VectorDataBase/MLModels/e5-small-v2/tokenizer_config.json ./MLModels/e5-small-v2/
COPY VectorDataBase/MLModels/e5-small-v2/vocab.txt ./MLModels/e5-small-v2/

# Download the large ONNX model
RUN apt-get update && apt-get install -y curl && \
    curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/model.onnx" \
    -o ./MLModels/e5-small-v2/model.onnx && \
    apt-get remove -y curl && apt-get autoremove -y && rm -rf /var/lib/apt/lists/*

# Copy sample data
COPY VectorDataBase/SampleData/ ./SampleData/

# Install OpenMP library (required by UMAP)
RUN apt-get update && apt-get install -y libgomp1 && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "SimiliVec.Api.dll"]