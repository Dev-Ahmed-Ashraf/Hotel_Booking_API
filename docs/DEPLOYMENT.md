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

For production Azure deployment, this repository includes Infrastructure as Code (Bicep) and GitHub Actions workflows with **OIDC** (no long-lived Azure passwords in GitHub).

### Architecture

| Component | Azure Service | Purpose |
|-----------|---------------|---------|
| API | App Service (Linux container) | Hosts the Docker image from ACR |
| Database | Azure SQL Database | Production SQL Server |
| Registry | Azure Container Registry | Stores versioned API images |
| Secrets | Azure Key Vault | Connection strings, JWT, Stripe keys |
| Monitoring | Application Insights + Log Analytics | Logs, metrics, alerts |

### One-time setup

#### 1. Install tools

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Bicep CLI](https://learn.microsoft.com/azure/azure-resource-manager/bicep/install) (included with recent Azure CLI)

#### 2. Configure GitHub OIDC (recommended)

Run from PowerShell (replace values):

```powershell
./scripts/setup-azure-oidc.ps1 `
  -SubscriptionId "<your-subscription-id>" `
  -ResourceGroup "rg-hotelbooking-prod" `
  -GitHubOrg "<your-github-org>" `
  -GitHubRepo "Hotel_Booking_API"
```

Add the printed values as GitHub Actions **secrets**:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | App Registration client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_RESOURCE_GROUP` | e.g. `rg-hotelbooking-prod` |
| `AZURE_WEBAPP_NAME` | From Bicep output after infra deploy |
| `SQL_ADMIN_LOGIN` | SQL admin username |
| `SQL_ADMIN_PASSWORD` | Strong SQL password |
| `JWT_SECRET_KEY` | Long random JWT signing key (32+ chars) |
| `STRIPE_API_KEY` | Optional |
| `STRIPE_WEBHOOK_SECRET` | Optional |

Add GitHub Actions **variables**:

| Variable | Example |
|----------|---------|
| `CORS_ALLOWED_ORIGINS` | `https://your-frontend.com,http://localhost:3000` |
| `ACR_NAME` | From Bicep output (`acrName`) |

Create a GitHub **environment** named `production` (and optionally `staging`) under Settings → Environments.

#### 3. Deploy infrastructure

**Option A — GitHub Actions (recommended)**

1. Go to **Actions → Deploy Infrastructure → Run workflow**
2. Choose `prod` or `staging`
3. Copy outputs: `webAppName`, `acrName`, `webAppUrl`
4. Set `AZURE_WEBAPP_NAME` and `ACR_NAME` in GitHub secrets/variables

**Option B — Azure CLI locally**

```bash
az login
az group create --name rg-hotelbooking-prod --location eastus

export SQL_ADMIN_PASSWORD='YourStrong!Passw0rd'
export JWT_SECRET_KEY='your-long-random-jwt-secret-key-at-least-32-chars'

az deployment group create \
  --resource-group rg-hotelbooking-prod \
  --template-file infra/main.bicep \
  --parameters @infra/main.prod.bicepparam \
  --parameters sqlAdminPassword="$SQL_ADMIN_PASSWORD" jwtSecretKey="$JWT_SECRET_KEY"
```

### Continuous deployment

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `ci.yml` | Push / PR to `main` | Build, test, verify Docker image |
| `infra.yml` | Manual | Deploy/update Azure resources |
| `deploy.yml` | Push to `main` | Build image → push to ACR → deploy to App Service |

After infrastructure is in place, every push to `main` runs tests, builds a Docker image tagged with the Git commit SHA, deploys to App Service, and verifies `/health`.

### Post-deployment checks

```bash
# Stream logs
az webapp log tail --name <web-app-name> --resource-group rg-hotelbooking-prod

# Health endpoints
curl https://<web-app-name>.azurewebsites.net/health
curl https://<web-app-name>.azurewebsites.net/health/ready

# Swagger
open https://<web-app-name>.azurewebsites.net/swagger
```

### Security best practices (included)

- Managed Identity for ACR pull and Key Vault access (no registry passwords on the Web App)
- GitHub OIDC instead of storing service principal secrets in `AZURE_CREDENTIALS`
- Secrets stored in Key Vault, referenced by App Service settings
- HTTPS-only Web App, TLS 1.2+, health check path configured
- Immutable image tags (`github.sha`) for each deployment

### Manual fallback (legacy)

If you prefer portal-based setup without Bicep:

```bash
az group create --name HotelBookingRG --location eastus
az appservice plan create --name HotelBookingPlan --resource-group HotelBookingRG --sku B1 --is-linux
az webapp create --resource-group HotelBookingRG --plan HotelBookingPlan --name hotelbooking-<unique> --deployment-container-image-name <acr>.azurecr.io/hotel-booking-api:latest
```

Configure app settings for connection string, JWT, and CORS in the Azure Portal or via CLI.

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
