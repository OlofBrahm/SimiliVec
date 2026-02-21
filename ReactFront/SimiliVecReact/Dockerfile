# Stage 1: Build the React app
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build argument for API URL
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

# Build the app
RUN npm run build

# Stage 2: Production with nginx
FROM nginx:alpine

# Copy built files from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Verify index.html exists
RUN test -f /usr/share/nginx/html/index.html || (echo "ERROR: index.html not found!" && exit 1)

# Copy custom nginx config template
COPY nginx.conf /etc/nginx/templates/default.conf.template

# nginx will automatically substitute $PORT in template files
ENV PORT=80

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
