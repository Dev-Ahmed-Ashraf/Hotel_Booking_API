param location string
param appServicePlanName string
param webAppName string
param appServicePlanSku string
param acrLoginServer string
param keyVaultUri string
param connectionStringSecretName string
param jwtSecretName string
param stripeApiKeySecretName string
param stripeWebhookSecretName string
param appInsightsConnectionString string
param corsAllowedOrigins string

var imageName = '${acrLoginServer}/hotel-booking-api:latest'
var keyVaultReferencePrefix = '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/'
var corsOrigins = [for (origin, index) in split(corsAllowedOrigins, ','): {
  name: 'Cors__AllowedOrigins__${index}'
  value: origin
}]

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: appServicePlanSku
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOCKER|${imageName}'
      acrUseManagedIdentityCreds: true
      alwaysOn: false
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      appSettings: concat([
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'DOCKER_ENABLE_CI'
          value: 'true'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '${keyVaultReferencePrefix}${connectionStringSecretName}/)'
        }
        {
          name: 'JwtSettings__SecretKey'
          value: '${keyVaultReferencePrefix}${jwtSecretName}/)'
        }
        {
          name: 'JwtSettings__Issuer'
          value: 'HotelBookingAPI'
        }
        {
          name: 'JwtSettings__Audience'
          value: 'HotelBookingAPIUsers'
        }
      ], corsOrigins, !empty(stripeApiKeySecretName) ? [
        {
          name: 'Stripe__ApiKey'
          value: '${keyVaultReferencePrefix}${stripeApiKeySecretName}/)'
        }
      ] : [], !empty(stripeWebhookSecretName) ? [
        {
          name: 'Stripe__WebhookSecret'
          value: '${keyVaultReferencePrefix}${stripeWebhookSecretName}/)'
        }
      ] : [])
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppPrincipalId string = webApp.identity.principalId
