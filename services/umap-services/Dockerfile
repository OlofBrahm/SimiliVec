# Use Python 3.11 slim image
FROM python:3.11-slim

# Set working directory
WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    gcc \
    g++ \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements first for better caching
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application
COPY UMAP_service.py .

# Railway will set this, but default to 8000 for local dev
ENV PORT=8000

# Expose the port
EXPOSE ${PORT}

# Run with uvicorn, binding to 0.0.0.0 and using Railway's PORT
CMD uvicorn UMAP_service:app --host 0.0.0.0 --port ${PORT}