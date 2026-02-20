# Railway Deployment Guide for SimiliVec

This guide walks through deploying all three services of the SimiliVec project to Railway.app.

## Architecture Overview

The project consists of three microservices:
1. **UMAP Service** (Python FastAPI) - Dimensionality reduction
2. **Vector API** (C# .NET 9) - Main backend with vector search
3. **React Frontend** (React + Vite) - User interface

## Prerequisites

- GitHub account with your SimiliVec repository pushed
- Railway.app account (sign up at https://railway.app)
- All Docker files committed to your repository

## Deployment Steps

### Step 1: Deploy UMAP Service (Internal Only)

This service doesn't need a public URL - it will only be accessed internally by the Vector API.

1. **Create New Project**
   - Go to Railway dashboard
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Choose your SimiliVec repository

2. **Configure UMAP Service**
   - Service name: `umap-service`
   - Root Directory: `services/umap-services`
   - Builder: Dockerfile
   - Dockerfile path: `services/umap-services/Dockerfile`

3. **No Environment Variables Needed**
   - This service has no configuration needed

4. **Deploy**
   - Railway will automatically build and deploy
   - Note the internal URL format: `http://umap-service.railway.internal:8000`

### Step 2: Deploy Vector API (Public URL)

This is your main C# backend API.

1. **Add New Service to Same Project**
   - In the same Railway project, click "+ New"
   - Select "GitHub Repo" (same repository)
   
2. **Configure Vector API Service**
   - Service name: `vector-api`
   - Root Directory: `/` (root of repository)
   - Builder: Dockerfile
   - Dockerfile path: `Dockerfile`

3. **Set Environment Variables**
   - Click on the service → "Variables" tab
   - Add these variables:
     ```
     UMAP_SERVICE_URL=http://umap-service.railway.internal:8000
     ```
   - Note: Don't set FRONTEND_URL yet - we'll add it after deploying the frontend

4. **Generate Public Domain**
   - Go to "Settings" tab
   - Under "Networking" → "Public Networking"
   - Click "Generate Domain"
   - **COPY THIS URL** - you'll need it for the frontend (e.g., `https://vector-api-production-xxxx.up.railway.app`)

5. **Deploy**
   - Railway will automatically build and deploy
   - Wait for deployment to complete (check logs)

### Step 3: Deploy React Frontend (Public URL)

This is your user-facing web application.

1. **Add New Service to Same Project**
   - In the same Railway project, click "+ New"
   - Select "GitHub Repo" (same repository)
   
2. **Configure React Service**
   - Service name: `react-frontend`
   - Root Directory: `ReactFront/SimiliVecReact`
   - Builder: Dockerfile
   - Dockerfile path: `ReactFront/SimiliVecReact/Dockerfile`

3. **Set Build Arguments**
   - Click on the service → "Variables" tab
   - Add this variable using the API URL from Step 2:
     ```
     VITE_API_URL=https://vector-api-production-xxxx.up.railway.app
     ```
   - Replace `xxxx` with your actual Vector API domain

4. **Generate Public Domain**
   - Go to "Settings" tab
   - Under "Networking" → "Public Networking"
   - Click "Generate Domain"
   - **COPY THIS URL** - this is your application URL (e.g., `https://react-frontend-production-yyyy.up.railway.app`)

5. **Deploy**
   - Railway will automatically build and deploy

### Step 4: Update CORS Configuration

Now that you have the frontend URL, go back and update the Vector API:

1. **Go to Vector API Service**
   - Select the `vector-api` service
   - Go to "Variables" tab

2. **Add Frontend URL**
   - Add this new variable:
     ```
     FRONTEND_URL=https://react-frontend-production-yyyy.up.railway.app
     ```
   - Replace `yyyy` with your actual frontend domain

3. **Redeploy**
   - Railway will automatically redeploy the API with the new CORS configuration

## Final Configuration Summary

After completing all steps, your environment variables should be:

### UMAP Service
- No environment variables

### Vector API
```
UMAP_SERVICE_URL=http://umap-service.railway.internal:8000
FRONTEND_URL=https://react-frontend-production-yyyy.up.railway.app
```

### React Frontend
```
VITE_API_URL=https://vector-api-production-xxxx.up.railway.app
```

## Testing Your Deployment

1. **Test the API**
   ```bash
   curl https://your-vector-api-url.railway.app/api/vector/nodes/pca
   ```
   Should return JSON array of PCA nodes

2. **Test the Frontend**
   - Open your frontend URL in a browser
   - Try searching for a document
   - Check browser console for any CORS errors

3. **Check Logs**
   - If something doesn't work, check Railway logs for each service
   - Click on service → "Deployments" → "View Logs"

## Common Issues

### CORS Errors
- Make sure `FRONTEND_URL` is set correctly in Vector API
- Frontend URL should not have trailing slash
- Wait for Vector API to redeploy after adding FRONTEND_URL

### API Not Connecting to UMAP Service
- Verify `UMAP_SERVICE_URL` uses `.railway.internal` domain
- Check UMAP service is running (green status in Railway)

### Build Failures
- Check Railway build logs for specific errors
- Ensure all Dockerfiles are in correct locations
- Verify ML models are included in the repository

### Frontend Shows Wrong API URL
- Build args are set at build time, not runtime
- If you change `VITE_API_URL`, trigger a rebuild
- Go to service → "Settings" → scroll down → "Restart"

## Railway Internal Networking

Services within the same Railway project can communicate using internal URLs:
- Format: `http://<service-name>.railway.internal:<port>`
- No authentication needed
- Faster and more secure than public URLs
- Only works within the same Railway project

## Costs

Railway offers:
- $5 free trial credit
- Usage-based pricing after trial
- Approximately $5-20/month for this project depending on traffic

## Next Steps

- Set up custom domain (optional)
- Configure environment-specific settings
- Set up CI/CD for automatic deployments
- Monitor application performance in Railway dashboard

## Troubleshooting Commands

Check if services are communicating:
```bash
# From Vector API logs, you should see UMAP service calls
# Check Railway logs for "UMAP service" or "http://umap-service"
```

Verify environment variables are set:
```bash
# In Railway, click service → Variables tab to see all vars
```

## Support

- Railway docs: https://docs.railway.app
- Railway Discord: https://discord.gg/railway
- GitHub Issues: [Your repository issues page]
