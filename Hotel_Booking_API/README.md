# Hotel Booking API

A professional, full-featured Hotel Booking API built with ASP.NET Core 8.0, implementing Clean Architecture, CQRS pattern, and industry best practices.

## 🏗️ Architecture

This API follows **Clean Architecture** principles with the following layers:

- **Domain Layer**: Core business entities, enums, and interfaces
- **Application Layer**: CQRS (Commands/Queries), DTOs, validators, and business logic
- **Infrastructure Layer**: Data access, external services, and repository implementations
- **API Layer**: Controllers, middleware, and filters

## ✨ Features

### Core Features
- **User Management**: Registration, authentication, and role-based authorization
- **Hotel Management**: CRUD operations for hotels with search and filtering
- **Room Management**: Room types, availability, and pricing
- **Booking System**: Complete booking workflow with status tracking
- **Review System**: User reviews and ratings for hotels
- **Payment Processing**: Payment tracking and status management

### Technical Features
- **JWT Authentication**: Secure token-based authentication
- **CQRS Pattern**: Command Query Responsibility Segregation with MediatR
- **Repository Pattern**: Generic repository with Unit of Work
- **FluentValidation**: Comprehensive input validation
- **AutoMapper**: Object-to-object mapping
- **Serilog**: Structured logging with file and console sinks
- **Swagger/OpenAPI**: Complete API documentation with JWT support
- **Global Exception Handling**: Consistent error responses
- **Soft Delete**: Data integrity with soft deletion
- **Pagination**: Efficient data retrieval with pagination
- **CORS Support**: Cross-origin resource sharing configuration

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB is configured by default)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Hotel_Booking_API
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   
   The default connection string in `appsettings.json` uses SQL Server:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=HotelBookingDb;Trusted_Connection=true;TrustServerCertificate=True;"
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API Base URL: `https://localhost:7062`
   - Swagger UI: `https://localhost:7062/swagger`
   - API Documentation: Available through Swagger UI

### Database Setup

The application automatically creates the database and seeds initial data on first run:

- **Admin User**: 
  - Email: `admin@hotelbooking.com`
  - Password: `Admin123!`
  - Role: Admin

- **Sample Hotels**: Grand Palace Hotel (New York), Ocean View Resort (Miami)
- **Sample Rooms**: Various room types with different pricing

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Hotels
- `GET /api/hotels` - Get all hotels (with pagination and search)
- `POST /api/hotels` - Create hotel (Admin only)
- `GET /api/hotels/{id}` - Get hotel by ID
- `PUT /api/hotels/{id}` - Update hotel (Admin only)
- `DELETE /api/hotels/{id}` - Delete hotel (Admin only)

### Rooms
- `GET /api/rooms/hotel/{hotelId}` - Get rooms by hotel
- `POST /api/rooms` - Create room (Admin/Hotel Manager)
- `GET /api/rooms/available` - Get available rooms with filters
- `PUT /api/rooms/{id}` - Update room (Admin/Hotel Manager)

### Bookings
- `GET /api/bookings` - Get user's bookings
- `POST /api/bookings` - Create new booking
- `PUT /api/bookings/{id}` - Update booking
- `DELETE /api/bookings/{id}` - Cancel booking

### Reviews
- `GET /api/reviews/hotel/{hotelId}` - Get hotel reviews
- `POST /api/reviews` - Create review
- `PUT /api/reviews/{id}` - Update review
- `DELETE /api/reviews/{id}` - Delete review

### Payments
- `POST /api/payments/process` - Process payment for booking
- `GET /api/payments/booking/{bookingId}` - Get payment by booking

## 🔐 Authentication

The API uses JWT (JSON Web Tokens) for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### User Roles
- **Customer (0)**: Can book rooms, write reviews
- **Admin (1)**: Full system access
- **Hotel Manager (2)**: Can manage hotel-specific data

## 🛠️ Configuration

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HotelBookingAPI",
    "Audience": "HotelBookingAPIUsers",
    "ExpirationMinutes": 60
  }
}
```

### Serilog Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### CORS Configuration
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200",
      "https://localhost:3000",
      "https://localhost:4200"
    ]
  }
}
```

## 🧪 Testing the API

Use the provided `Hotel_Booking.http` file with VS Code REST Client extension or any HTTP client:

1. **Register a new user**:
   ```http
   POST https://localhost:7062/api/auth/register
   Content-Type: application/json

   {
     "email": "john.doe@example.com",
     "password": "SecurePass123!",
     "firstName": "John",
     "lastName": "Doe",
     "role": 0
   }
   ```

2. **Login**:
   ```http
   POST https://localhost:7062/api/auth/login
   Content-Type: application/json

   {
     "email": "admin@hotelbooking.com",
     "password": "Admin123!"
   }
   ```

3. **Get hotels**:
   ```http
   GET https://localhost:7062/api/hotels?pageNumber=1&pageSize=10
   ```

## 📁 Project Structure

```
Hotel_Booking_API/
├── Domain/
│   ├── Entities/           # Core business entities
│   ├── Enums/             # Business enums
│   └── Interfaces/        # Domain interfaces
├── Application/
│   ├── Features/          # CQRS commands and queries
│   ├── DTOs/             # Data transfer objects
│   ├── Validators/       # FluentValidation validators
│   ├── Common/           # Shared application logic
│   └── Mappings/         # AutoMapper profiles
├── Infrastructure/
│   ├── Data/             # DbContext and configurations
│   ├── Repositories/     # Repository implementations
│   └── Services/         # Infrastructure services
├── Controllers/          # API controllers
├── Middleware/           # Custom middleware
└── Properties/           # Application properties
```

## 🔧 Development

### Adding New Features

1. **Domain Layer**: Add entities and enums
2. **Application Layer**: Create DTOs, commands/queries, and handlers
3. **Infrastructure Layer**: Add repository methods and services
4. **API Layer**: Create controllers and endpoints

### Database Migrations

The application uses `EnsureCreated()` for simplicity. For production, use EF Core migrations:

```bash
# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

## 🚀 Deployment

### Production Considerations

1. **Update JWT Secret Key**: Use a secure, randomly generated key
2. **Configure Production Database**: Update connection string
3. **Enable HTTPS**: Ensure SSL certificates are properly configured
4. **Configure Logging**: Set up appropriate log levels for production
5. **Environment Variables**: Use environment variables for sensitive configuration
6. **Rate Limiting**: Consider implementing rate limiting for production
7. **Health Checks**: Add health check endpoints

### Docker Support (Optional)

Create a `Dockerfile` for containerized deployment:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Hotel_Booking.csproj", "."]
RUN dotnet restore "./Hotel_Booking.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Hotel_Booking.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hotel_Booking.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hotel_Booking.dll"]
```

## 📝 API Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Email is required",
    "Password must be at least 8 characters"
  ]
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "items": [ ... ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": []
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions:
- Email: support@hotelbooking.com
- Documentation: Available through Swagger UI
- Issues: Create an issue in the repository

---

**Built with ❤️ using ASP.NET Core 8.0**
