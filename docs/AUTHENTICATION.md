# Authentication & Authorization Guide

## Overview
This document provides detailed information about the authentication and authorization mechanisms used in the Hotel Booking API.

## Authentication

The API uses JWT (JSON Web Tokens) for stateless authentication. All authenticated endpoints require a valid JWT token in the `Authorization` header.

### Token Structure

#### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### Payload
```json
{
  "sub": "user-id-here",
  "email": "user@example.com",
  "name": "John Doe",
  "roles": ["Customer", "Admin", "HotelManager"],
  "nbf": 1637424000,
  "exp": 1637510400,
  "iat": 1637424000
}
```

### Authentication Flow

1. **User Login**
   - Client sends credentials to `/api/auth/login`
   - Server validates credentials
   - If valid, issues JWT token

2. **Accessing Protected Resources**
   - Client includes token in `Authorization: Bearer <token>` header
   - Server validates token on each request
   - If valid, processes request; if not, returns 401 Unauthorized

3. **Token Refresh**
   - When access token is about to expire, client can request a new one using refresh token
   - Send refresh token to `/api/auth/refresh-token`
   - If valid, new access token is issued

## Authorization

### Roles

| Role | Permissions |
|------|-------------|
| **Customer** | Book rooms, manage own bookings |
| **HotelManager** | Manage hotels and rooms |
| **Admin** | Full system access |

### Role-Based Access Control (RBAC)

#### Controller Level
```csharp
[Authorize(Roles = "Admin,HotelManager")]
[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    // Controller actions
}
```

#### Action Level
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateHotel(CreateHotelCommand command)
{
    // Implementation
}
```

### Policy-Based Authorization

#### Policy Definition (Startup.cs)
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", 
        policy => policy.RequireRole("Admin"));
        
    options.AddPolicy("CanManageHotels", policy => 
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") || 
            context.User.IsInRole("HotelManager")));
});
```

#### Using Policies
```csharp
[Authorize(Policy = "CanManageHotels")]
public async Task<IActionResult> UpdateHotel(Guid id, UpdateHotelCommand command)
{
    // Implementation
}
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long",
    "Issuer": "HotelBookingAPI",
    "Audience": "HotelBookingClients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Environment Variables
For production, set these environment variables:
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__AccessTokenExpirationMinutes`
- `JwtSettings__RefreshTokenExpirationDays`

## API Endpoints

### Authentication

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourSecurePassword123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresIn": 3600,
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "name": "John Doe",
    "roles": ["User"]
  }
}
```

#### Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "token": "expired-token-here",
  "refreshToken": "refresh-token-here"
}
```

#### Logout
```http
POST /api/auth/revoke-token
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "refreshToken": "refresh-token-here"
}
```

## Security Best Practices

1. **Password Security**
   - Passwords are hashed using PBKDF2 with HMAC-SHA256
   - Minimum password length: 8 characters
   - Required: uppercase, lowercase, number, and special character

2. **Token Security**
   - Access tokens have a short expiration (default: 60 minutes)
   - Refresh tokens are stored securely with hashed values
   - Token validation includes issuer and audience validation

3. **Rate Limiting**
   - Login attempts are rate-limited
   - Suspicious activity triggers account lockout

4. **HTTPS**
   - All authentication endpoints require HTTPS
   - HSTS is enabled in production

5. **CORS**
   - Configured to allow requests only from trusted origins
   - Credentials are only sent with same-origin requests

## Testing Authentication

### Using Swagger UI
1. Click the "Authorize" button
2. Enter `Bearer your-jwt-token` in the value field
3. Click "Authorize"

### Using cURL
```bash
curl -X GET "https://api.example.com/api/protected-endpoint" \
  -H "Authorization: Bearer your-jwt-token"
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Token is missing or invalid
   - Token has expired
   - Invalid signature

2. **403 Forbidden**
   - User doesn't have required role/permission
   - Account is locked out

3. **400 Bad Request**
   - Invalid request format
   - Missing required fields

### Logs
Check application logs for detailed error messages. Look for entries with category:
- `Microsoft.AspNetCore.Authentication`
- `Microsoft.AspNetCore.Authorization`

## References
- [JWT.IO](https://jwt.io/)
- [Microsoft Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
