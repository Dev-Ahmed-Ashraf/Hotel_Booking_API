using FluentValidation;
using Hotel_Booking_API.Application.Common.Behaviors;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.Mappings;
using Hotel_Booking_API.Application.Validators.AuthValidators;
using Hotel_Booking_API.Domain.Interfaces;
using Hotel_Booking_API.Infrastructure.Caching;
using Hotel_Booking_API.Infrastructure.Data;
using Hotel_Booking_API.Infrastructure.Repositories;
using Hotel_Booking_API.Infrastructure.Services;
using Hotel_Booking_API.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

namespace Hotel_Booking_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Temporary bootstrap logger (logs during startup)
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Hotel Booking API");

                // Create the web app builder (loads all configurations automatically)
                var builder = WebApplication.CreateBuilder(args);

                // Proper Serilog configuration (uses builder.Configuration)
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateLogger();

                builder.Host.UseSerilog();

                // Configure services & middleware as usual
                ConfigureServices(builder.Services, builder.Configuration);
                var app = builder.Build();
                ConfigureMiddleware(app);
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
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

            // Stripe options and service
            services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
            services.AddSingleton<IStripeService, StripeService>();

            // Email service configuration
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // Configure MediatR for CQRS pattern
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Configure AutoMapper for object mapping
            services.AddAutoMapper(typeof(MappingProfile));

            // Configure FluentValidation with custom behaviors
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            // Response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            // Output caching
            services.AddOutputCache();

            // Rate limiting (fixed window per IP)
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            // Health checks
            services.AddHealthChecks();

            // Memory cache and cache settings
            services.Configure<CacheSettings>(configuration.GetSection("MemoryCache"));
            services.AddMemoryCache(options =>
            {
                var settings = configuration.GetSection("MemoryCache").Get<CacheSettings>();
                if (settings != null && settings.SizeLimitMB > 0)
                {
                    options.SizeLimit = settings.SizeLimitMB * 1024L * 1024L;
                }
            });
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<ICacheInvalidator, CacheInvalidator>();


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

                // ????? ???? ??? ?????
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

                        // ????? ??????? ??? ??? endpoint
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

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // Correlation ID
            app.Use(async (context, next) =>
            {
                const string headerName = "X-Correlation-ID";
                if (!context.Request.Headers.TryGetValue(headerName, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                    context.Request.Headers[headerName] = correlationId;
                }
                context.Response.Headers[headerName] = correlationId!
                    .ToString();
                await next();
            });

            // Enable CORS with the configured policy
            app.UseCors("AllowSpecificOrigins");

            // Response compression
            app.UseResponseCompression();

            // Rate limiting
            app.UseRateLimiter();

            // Output cache
            app.UseOutputCache();

            // Add authentication and authorization middleware
            // Note: Order is important - Authentication must come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controller routes
            app.MapControllers();

            // Health checks
            app.MapHealthChecks("/health");

            // Apply migrations in Development only
            using (var scope = app.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                if (env.IsDevelopment())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();
                }
            }
        }
    }
}
