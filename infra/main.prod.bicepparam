using '../main.bicep'

param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD', 'Aa@010271776086311')
param jwtSecretKey = readEnvironmentVariable('JWT_SECRET_KEY', 'YourSuperSecretKeyThatIsAtLeast32CharactersLong!')
param corsAllowedOrigins = 'https://localhost:3000'
