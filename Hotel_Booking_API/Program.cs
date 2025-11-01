
// Core ASP.NET Core and Entity Framework imports
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// Third-party libraries for validation, logging, and CQRS
using FluentValidation;
using Serilog;
using MediatR;

// Application layer imports - Clean Architecture pattern
using Hotel_Booking_API.Infrastructure.Data;           // Database context
using Hotel_Booking_API.Infrastructure.Repositories;  // Repository pattern implementation
using Hotel_Booking_API.Infrastructure.Services;      // JWT service and other infrastructure services
using Hotel_Booking_API.Domain.Interfaces;             // Domain layer interfaces
using Hotel_Booking_API.Application.Mappings;         // AutoMapper profiles
using Hotel_Booking_API.Application.Common.Behaviors;  // MediatR pipeline behaviors
using Hotel_Booking_API.Middleware;                   // Custom middleware
using Hotel_Booking.Application.Validators.AuthValidators;
using Hotel_Booking.Domain.Interfaces;
using Hotel_Booking.Infrastructure.Repositories;

namespace Hotel_Booking_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog for application logging
            // Reads configuration from appsettings.json and sets up logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build())
                .CreateLogger();


            try
            {
                Log.Information("Starting Hotel Booking API");

                // Create and configure the web application builder
                var builder = WebApplication.CreateBuilder(args);

                // Use Serilog for logging throughout the application
                builder.Host.UseSerilog();

                // Configure application services and dependency injection
                ConfigureServices(builder.Services, builder.Configuration);

                // Build the application
                var app = builder.Build();

                // Configure the HTTP request pipeline
                ConfigureMiddleware(app);

                // Run the application
                app.Run();
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions that might occur during startup
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                // Ensure all logs are properly flushed before exiting
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Configures the application services and dependency injection container
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="configuration">The application configuration</param>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Entity Framework Core with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register generic repository and unit of work patterns
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register JWT service for authentication
            services.AddScoped<IJwtService, JwtService>();

            // Configure MediatR for CQRS pattern
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Configure AutoMapper for object mapping
            services.AddAutoMapper(typeof(MappingProfile));

            // Configure FluentValidation with custom behaviors
            services.AddValidatorsFromAssembly(typeof(RegisterUserValidator).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });


            // Configure CORS (Cross-Origin Resource Sharing)
            var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0];
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(corsOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Configure JWT authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No clock skew for strict token validation
                };

                // تخصيص الرد عند الخطأ
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = "Authentication is required. Please login to continue."
                        });

                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var path = context.Request.Path.Value?.ToLower();
                        string message;

                        // تخصيص الرسائل حسب الـ endpoint
                        if (path.Contains("/bookings"))
                        {
                            message = "Only Admins and Customers can perform this action.";
                        }
                        else if (path.Contains("/hotels"))
                        {
                            message = "Only Admins can manage hotels.";
                        }
                        else
                        {
                            message = "You do not have permission to perform this action.";
                        }

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            message
                        });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

            // Add authorization services
            services.AddAuthorization();

            // Configure API versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // Returns API version in response headers
            });

            // Configure Swagger/OpenAPI documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Hotel Booking API",
                    Version = "v1",
                    Description = "A professional hotel booking API with full features",
                    Contact = new OpenApiContact
                    {
                        Name = "Hotel Booking API Team",
                        Email = "support@hotelbooking.com"
                    }
                });

                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                // Include XML documentation for API endpoints
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline
        /// </summary>
        /// <param name="app">The web application instance</param>
        private static void ConfigureMiddleware(WebApplication app)
        {
            // Global exception handling middleware (must be first in pipeline)
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline based on environment
            if (app.Environment.IsDevelopment())
            {
                // Enable Swagger UI in development
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Booking API V1");
                    //c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
                });
            }

            // Redirect HTTP to HTTPS for security
            app.UseHttpsRedirection();

            // Enable CORS with the configured policy
            app.UseCors("AllowSpecificOrigins");

            // Add authentication and authorization middleware
            // Note: Order is important - Authentication must come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controller routes
            app.MapControllers();

            // Ensure database is created (for development)
            // In production, use migrations instead
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            }
        }
    }
}
