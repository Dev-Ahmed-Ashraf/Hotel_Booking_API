targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Environment name used in resource naming (e.g. prod, staging).')
@minLength(2)
@maxLength(10)
param environmentName string = 'prod'

@description('Short project prefix used in resource names.')
@minLength(3)
@maxLength(12)
param projectName string = 'hotelbooking'

@description('SQL Server administrator login.')
param sqlAdminLogin string

@secure()
@description('SQL Server administrator password.')
param sqlAdminPassword string

@secure()
@description('JWT signing key (minimum 32 characters recommended).')
param jwtSecretKey string

@description('App Service plan SKU (e.g. B1, S1, P1v3).')
param appServicePlanSku string = 'B1'

@description('Azure SQL database SKU name.')
param sqlDatabaseSku string = 'Basic'

@description('ACR SKU (Basic or Standard).')
param acrSku string = 'Basic'

@description('CORS origins as comma-separated URLs.')
param corsAllowedOrigins string = 'https://localhost:3000'

@secure()
param stripeApiKey string = ''

@secure()
param stripeWebhookSecret string = ''

var uniqueSuffix = uniqueString(resourceGroup().id, projectName, environmentName)
var acrName = toLower('${projectName}${environmentName}${uniqueSuffix}')
var keyVaultName = toLower('kvhotel${take(uniqueString(resourceGroup().id), 8)}')
var sqlServerName = toLower('sql-${projectName}-${environmentName}-${take(uniqueSuffix, 6)}')
var appServicePlanName = 'asp-${projectName}-${environmentName}'
var webAppName = toLower('app-${projectName}-${environmentName}-${take(uniqueSuffix, 6)}')
var logAnalyticsName = 'log-${projectName}-${environmentName}'
var appInsightsName = 'appi-${projectName}-${environmentName}'
var databaseName = 'HotelBookingDb'

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    location: location
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql-deployment'
  params: {
    location: location
    sqlServerName: sqlServerName
    databaseName: databaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    sqlDatabaseSku: sqlDatabaseSku
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault-deployment'
  params: {
    location: location
    keyVaultName: keyVaultName
    jwtSecretKey: jwtSecretKey
    sqlConnectionString: sql.outputs.connectionString
    stripeApiKey: stripeApiKey
    stripeWebhookSecret: stripeWebhookSecret
  }
}

module acr 'modules/acr.bicep' = {
  name: 'acr-deployment'
  params: {
    location: location
    acrName: acrName
    acrSku: acrSku
  }
}

module appService 'modules/app-service.bicep' = {
  name: 'app-service-deployment'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    appServicePlanSku: appServicePlanSku
    acrLoginServer: acr.outputs.loginServer
    keyVaultUri: keyVault.outputs.keyVaultUri
    connectionStringSecretName: keyVault.outputs.connectionStringSecretName
    jwtSecretName: keyVault.outputs.jwtSecretName
    stripeApiKeySecretName: keyVault.outputs.stripeApiKeySecretName
    stripeWebhookSecretName: keyVault.outputs.stripeWebhookSecretName
    appInsightsConnectionString: monitoring.outputs.connectionString
    corsAllowedOrigins: corsAllowedOrigins
  }
}

output acrLoginServer string = acr.outputs.loginServer
output acrName string = acr.outputs.name
output webAppName string = appService.outputs.webAppName
output webAppUrl string = appService.outputs.webAppUrl
output keyVaultName string = keyVault.outputs.keyVaultName
output sqlServerFqdn string = sql.outputs.serverFqdn
output appInsightsConnectionString string = monitoring.outputs.connectionString
