# Deployment Guide

This guide provides instructions for deploying the Hotel Booking API in different environments.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Environment Variables](#environment-variables)
- [Local Development](#local-development)
- [Docker Deployment](#docker-deployment)
- [Azure App Service](#azure-app-service)
- [Kubernetes](#kubernetes)
- [Database Migrations](#database-migrations)
- [Monitoring](#monitoring)
- [Backup and Recovery](#backup-and-recovery)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### For All Deployments
- .NET 8.0 SDK
- SQL Server 2019+
- Git

### For Containerized Deployments
- Docker 20.10+
- Docker Compose 1.29+
- (Optional) Kubernetes CLI (kubectl)

### For Cloud Deployments
- Azure CLI (for Azure deployments)
- AWS CLI (for AWS deployments)
- Google Cloud SDK (for GCP deployments)

## Environment Variables

Create a `.env` file in the project root with the following variables:

```env
# Database
DB_SERVER=your-db-server
DB_NAME=HotelBooking
DB_USER=db-user
DB_PASSWORD=your-secure-password

# JWT Authentication
JWT_SECRET=your-jwt-secret-key
JWT_ISSUER=HotelBookingAPI
JWT_AUDIENCE=HotelBookingClients
JWT_EXPIRE_MINUTES=60

# CORS
CORS_ORIGINS=https://your-frontend.com,http://localhost:3000

# Email Settings
SMTP_HOST=smtp.sendgrid.net
SMTP_PORT=587
SMTP_USER=apikey
SMTP_PASSWORD=your-sendgrid-api-key
SMTP_SenderEmail=noreply@yourdomain.com
SMTP_SenderName="Hotel Booking"
SMTP_EnableSsl=true

# Stripe (for payments)
STRIPE_ApiKey= Your-STRIPE_ApiKey,
STRIPE_SECRET_KEY=your-stripe-secret-key
STRIPE_WEBHOOK_SECRET=your-stripe-webhook-secret
STRIPE_PUBLISHABLE_KEY=your-stripe-publishable-key

# Application Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

## Local Development

### 1. Clone the Repository
```bash
git clone https://github.com/Dev-Ahmed-Ashraf/Hotel_Booking_API.git
cd Hotel_Booking_API
```

### 2. Configure Environment
1. Copy `.env.example` to `.env`
2. Update the values in `.env`
3. For development, you can use SQL Server LocalDB

### 3. Run Database Migrations
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

### 4. Run the Application
```bash
# Development (watch mode)
dotnet watch --project src/API run

# Or production mode
dotnet run --project src/API --launch-profile Production
```

## Docker Deployment

### 1. Build the Docker Image
```bash
docker build -t hotel-booking-api .
```

### 2. Run with Docker Compose
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### 3. Verify the Deployment
```bash
docker ps
docker logs hotel-booking-api
```

## Azure App Service

### 1. Create Azure Resources
```bash
# Login to Azure
az login

# Create Resource Group
az group create --name HotelBookingRG --location eastus

# Create App Service Plan
az appservice plan create --name HotelBookingPlan --resource-group HotelBookingRG --sku B1 --is-linux

# Create Web App
az webapp create --resource-group HotelBookingRG --plan HotelBookingPlan --name hotelbooking-$(date +%s) --runtime "DOTNET:8.0"

# Configure App Settings
az webapp config appsettings set --resource-group HotelBookingRG --name <app-name> --settings \
  "ConnectionStrings__DefaultConnection=Server=tcp:<server-name>.database.windows.net,1433;Database=HotelBooking;User ID=<db-user>;Password=<db-password>;Encrypt=true;" \
  "JwtSettings__SecretKey=<your-jwt-secret>"
```

### 2. Deploy from GitHub
1. Go to Azure Portal > Your App Service > Deployment Center
2. Select GitHub as source
3. Authorize and select your repository and branch
4. Configure the build process
5. Save and deploy

## Kubernetes

### 1. Create Kubernetes Cluster
```bash
# Example for AKS
az aks create --resource-group HotelBookingRG --name HotelBookingCluster --node-count 2 --enable-addons monitoring --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group HotelBookingRG --name HotelBookingCluster
```

### 2. Deploy to Kubernetes
```bash
# Create namespace
kubectl create namespace hotel-booking

# Create secrets
kubectl create secret generic hotel-booking-secrets --from-env-file=.env -n hotel-booking

# Deploy application
kubectl apply -f k8s/

# Check deployment status
kubectl get all -n hotel-booking
```

## Database Migrations

### 1. Generate Migrations
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API
```

### 2. Apply Migrations
```bash
# For local development
dotnet ef database update --project src/Infrastructure --startup-project src/API

# For production (using EF Core tools in Docker)
docker-compose -f docker-compose.migrations.yml up
```

## Monitoring

### Application Insights
1. Create an Application Insights resource in Azure
2. Add the Instrumentation Key to your app settings:
   ```
   ApplicationInsights__InstrumentationKey=your-instrumentation-key
   ```

### Health Checks
Access the health check endpoint:
```
GET /health
```

### Logging
Logs are written to:
- Console (stdout/stderr)
- Application Insights (if configured)
- File system (in development)

## Backup and Recovery

### Database Backup
```sql
-- Full backup
BACKUP DATABASE [HotelBooking] 
TO DISK = N'/var/opt/mssql/backup/HotelBooking_Full.bak' 
WITH INIT, COMPRESSION, STATS = 10;

-- Transaction log backup
BACKUP LOG [HotelBooking] 
TO DISK = N'/var/opt/mssql/backup/HotelBooking_Log.trn' 
WITH INIT, COMPRESSION, STATS = 10;
```

### Automated Backups with Azure
```bash
# Create a storage account
az storage account create --name hotelbookingbackup --resource-group HotelBookingRG --location eastus --sku Standard_LRS

# Create a container
az storage container create --name backups --account-name hotelbookingbackup

# Create a backup policy
az sql db backup policy set --resource-group HotelBookingRG --server <server-name> --name HotelBooking \
  --retention 30 --backup-policy-type LTR
```

## Scaling

### Vertical Scaling (Scale Up)
- For Azure App Service: Increase the App Service Plan tier
- For Kubernetes: Adjust resource requests/limits in the deployment YAML

### Horizontal Scaling (Scale Out)
```bash
# Scale web app to 3 instances
az webapp scale --resource-group HotelBookingRG --name <app-name> --instances 3

# Or for Kubernetes
kubectl scale deployment/hotel-booking-api --replicas=3 -n hotel-booking
```

## SSL/TLS

### Let's Encrypt with Azure
```bash
# Enable HTTPS only
az webapp update --https-only true --name <app-name> --resource-group HotelBookingRG

# Configure SSL binding
az webapp config ssl bind --certificate-thumbprint <thumbprint> --ssl-type SNI \
  --name <app-name> --resource-group HotelBookingRG
```

## Troubleshooting

### Common Issues

#### Database Connection Issues
- Verify connection string
- Check if the database server is accessible
- Ensure firewall rules allow the connection

#### Application Startup Errors
- Check application logs
- Verify all required environment variables are set
- Check database migrations have been applied

#### Performance Issues
- Check database query performance
- Review application logs for slow requests
- Monitor resource usage (CPU, memory, disk I/O)

### Logs

#### Local Development
```bash
# View application logs
dotnet run --project src/API

# View container logs
docker logs hotel-booking-api
```

#### Azure App Service
```bash
# Stream logs
az webapp log tail --name <app-name> --resource-group HotelBookingRG

# Download logs
az webapp log download --log-file app-logs.zip --name <app-name> --resource-group HotelBookingRG
```

#### Kubernetes
```bash
# View pod logs
kubectl logs -f <pod-name> -n hotel-booking

# Describe pod for more details
kubectl describe pod <pod-name> -n hotel-booking
```

## Maintenance

### Updating the Application
1. Pull the latest changes
2. Run database migrations if needed
3. Rebuild and restart the application

### Monitoring
- Set up alerts for critical issues
- Monitor application performance
- Review security logs regularly

## Rollback Plan

### Manual Rollback
1. Revert to previous deployment
2. Run database rollback if needed
3. Verify application functionality

### Automated Rollback
- Configure health probes in Kubernetes
- Set up deployment strategies (e.g., rolling updates with max unavailable)
- Use feature flags for risky changes

## Security Considerations

### Network Security
- Use Network Security Groups (NSGs)
- Implement VNet Integration
- Use Private Endpoints for PaaS services

### Secrets Management
- Use Azure Key Vault or similar
- Never commit secrets to source control
- Rotate secrets regularly

### Compliance
- Enable auditing for sensitive operations
- Implement proper access controls
- Regular security assessments
