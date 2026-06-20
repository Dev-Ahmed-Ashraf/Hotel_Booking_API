param location string
param keyVaultName string

@secure()
param jwtSecretKey string

@secure()
param sqlConnectionString string

@secure()
param stripeApiKey string = ''

@secure()
param stripeWebhookSecret string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enabledForTemplateDeployment: true
  }
}

resource connectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--DefaultConnection'
  properties: {
    value: sqlConnectionString
  }
}

resource jwtSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JwtSettings--SecretKey'
  properties: {
    value: jwtSecretKey
  }
}

resource stripeApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(stripeApiKey)) {
  parent: keyVault
  name: 'Stripe--ApiKey'
  properties: {
    value: stripeApiKey
  }
}

resource stripeWebhookSecretResource 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(stripeWebhookSecret)) {
  parent: keyVault
  name: 'Stripe--WebhookSecret'
  properties: {
    value: stripeWebhookSecret
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output jwtSecretName string = jwtSecret.name
output stripeApiKeySecretName string = !empty(stripeApiKey) ? stripeApiKeySecret.name : ''
output stripeWebhookSecretName string = !empty(stripeWebhookSecret) ? stripeWebhookSecretResource.name : ''
output connectionStringSecretName string = connectionStringSecret.name
