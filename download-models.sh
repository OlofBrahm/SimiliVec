#!/bin/bash
# Download E5-small-v2 model from Hugging Face

MODEL_DIR="VectorDataBase/MLModels/e5-small-v2"
mkdir -p "$MODEL_DIR"

# Download model files
curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/model.onnx" -o "$MODEL_DIR/model.onnx"
curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/tokenizer.json" -o "$MODEL_DIR/tokenizer.json"
curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/tokenizer_config.json" -o "$MODEL_DIR/tokenizer_config.json"
curl -L "https://huggingface.co/intfloat/e5-small-v2/resolve/main/vocab.txt" -o "$MODEL_DIR/vocab.txt"

echo "Models downloaded successfully"
