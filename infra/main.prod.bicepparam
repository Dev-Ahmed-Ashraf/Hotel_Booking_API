using '../main.bicep'

param sqlAdminLogin = 'sqladmin'
param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD', 'ChangeMe-Strong-Passw0rd!')
param jwtSecretKey = readEnvironmentVariable('JWT_SECRET_KEY', 'ChangeMe-Use-A-Long-Random-Jwt-Secret-Key-32+')
param corsAllowedOrigins = 'https://localhost:3000'
