<#
.SYNOPSIS
  One-time Azure setup for GitHub Actions OIDC deployment.

.DESCRIPTION
  Creates an App Registration, federated credential for GitHub Actions,
  and assigns Contributor on the target resource group.

.EXAMPLE
  ./scripts/setup-azure-oidc.ps1 `
    -SubscriptionId "109862a1-f654-4905-9ae0-ee5a7d7ba527" `
    -ResourceGroup "rg-hotelbooking-prod" `
    -GitHubOrg "Dev-Ahmed-Ashraf" `
    -GitHubRepo "Hotel_Booking_API"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true)]
    [string]$GitHubOrg,

    [Parameter(Mandatory = $true)]
    [string]$GitHubRepo,

    [string]$AppName = "github-hotel-booking-deploy",
    [string]$Location = "italynorth"
)

$ErrorActionPreference = "Stop"

Write-Host "Setting subscription..."
az account set --subscription $SubscriptionId

Write-Host "Ensuring resource group exists..."
az group create --name $ResourceGroup --location $Location | Out-Null

Write-Host "Creating App Registration..."
$appId = az ad app create --display-name $AppName --query appId -o tsv
$spObjectId = az ad sp create --id $appId --query id -o tsv

Write-Host "Creating federated credential for GitHub Actions..."
$credential = @{
    name        = "github-main"
    issuer      = "https://token.actions.githubusercontent.com"
    subject     = "repo:${GitHubOrg}/${GitHubRepo}:ref:refs/heads/main"
    description = "GitHub Actions deployment from main branch"
    audiences   = @("api://AzureADTokenExchange")
} | ConvertTo-Json -Compress

az ad app federated-credential create --id $appId --parameters $credential | Out-Null

Write-Host "Assigning Contributor on resource group..."
$rgScope = az group show --name $ResourceGroup --query id -o tsv
az role assignment create `
    --assignee-object-id $spObjectId `
    --assignee-principal-type ServicePrincipal `
    --role Contributor `
    --scope $rgScope | Out-Null

Write-Host ""
Write-Host "Add these GitHub repository secrets:"
Write-Host "  AZURE_CLIENT_ID       = $appId"
Write-Host "  AZURE_TENANT_ID       = $(az account show --query tenantId -o tsv)"
Write-Host "  AZURE_SUBSCRIPTION_ID = $SubscriptionId"
Write-Host "  AZURE_RESOURCE_GROUP  = $ResourceGroup"
Write-Host ""
Write-Host "Also add application secrets before running infra workflow:"
Write-Host "  SQL_ADMIN_LOGIN, SQL_ADMIN_PASSWORD, JWT_SECRET_KEY"
Write-Host "  STRIPE_API_KEY, STRIPE_WEBHOOK_SECRET (optional)"
Write-Host ""
Write-Host "GitHub repository variables (Settings -> Secrets and variables -> Actions -> Variables):"
Write-Host "  CORS_ALLOWED_ORIGINS = https://your-frontend.com"
Write-Host "  ACR_NAME             = (set after first infra deployment output)"
