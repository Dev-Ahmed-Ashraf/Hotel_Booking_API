using FluentValidation;
using Hotel_Booking_API.Application.Common.Behaviors;
using Hotel_Booking_API.Application.Common.Interfaces;
using Hotel_Booking_API.Application.Features.Payments.Services;
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
    public partial class Program
    {  
        public static WebApplication BuildWebApp(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Hotel Booking API");
                var builder = WebApplication.CreateBuilder(args);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateLogger();

                builder.Host.UseSerilog();
                ConfigureServices(builder.Services, builder.Configuration);
                var app = builder.Build();

                if (!app.Environment.IsEnvironment("Test"))
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }

                ConfigureMiddleware(app);
                return app;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static void Main(string[] args)
        {
            BuildWebApp(args).Run();
        }

        /// <summary>
        /// Registers all application services in the DI container.
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // -----------------------------
            // Database Configuration
            // -----------------------------
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // -----------------------------
            // Repositories & Unit of Work
            // -----------------------------
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // -----------------------------
            // Authentication: JWT Service
            // -----------------------------
            services.AddScoped<IJwtService, JwtService>();

            // -----------------------------
            // Stripe Payment Integration
            // -----------------------------
            services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
            services.AddSingleton<IStripeService, StripeService>();
            services.AddScoped<IPaymentUpdateService, PaymentUpdateService>();

            // -----------------------------
            // Email Service
            // -----------------------------
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailService, EmailService>();

            // -----------------------------
            // MediatR (CQRS)
            // -----------------------------
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // -----------------------------
            // AutoMapper
            // -----------------------------
            services.AddAutoMapper(typeof(MappingProfile));

            // -----------------------------
            // FluentValidation + Pipeline Behaviors
            // -----------------------------
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            // -----------------------------
            // Controllers + JSON Settings
            // -----------------------------
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            // -----------------------------
            // Response Compression
            // -----------------------------
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            // -----------------------------
            // Output Caching
            // -----------------------------
            services.AddOutputCache();

            // -----------------------------
            // Rate Limiting (Per-IP)
            // -----------------------------
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,       // Max 100 requests
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            // -----------------------------
            // Health Checks
            // -----------------------------
            services.AddHealthChecks();

            // -----------------------------
            // Memory Cache
            // -----------------------------
            services.Configure<CacheSettings>(configuration.GetSection("MemoryCache"));
            services.AddMemoryCache(options =>
            {
                var settings = configuration.GetSection("MemoryCache").Get<CacheSettings>();
                if (settings?.SizeLimitMB > 0)
                {
                    options.SizeLimit = settings.SizeLimitMB * 1024L * 1024L;
                }
            });

            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<ICacheInvalidator, CacheInvalidator>();

            // -----------------------------
            // CORS Configuration
            // -----------------------------
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

            // -----------------------------
            // JWT Authentication
            // -----------------------------
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    ClockSkew = TimeSpan.Zero  // No time tolerance
                };

                // Custom Unauthorized / Forbidden messages
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        return context.Response.WriteAsync(
                            System.Text.Json.JsonSerializer.Serialize(new
                            {
                                success = false,
                                message = "Authentication is required. Please login to continue."
                            }));
                    },

                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        return context.Response.WriteAsync(
                            System.Text.Json.JsonSerializer.Serialize(new
                            {
                                success = false,
                                message = "You do not have permission to perform this action."
                            }));
                    }
                };
            });

            services.AddAuthorization();

            // -----------------------------
            // API Versioning
            // -----------------------------
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            // -----------------------------
            // Swagger / OpenAPI
            // -----------------------------
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Hotel Booking API",
                    Version = "v1",
                    Description = "A professional hotel booking API with full features"
                });

                // JWT Auth in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization. Example: \"Bearer {token}\"",
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

                // Include XML Comments
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });
        }

        /// <summary>
        /// Configures the HTTP request pipeline (middleware).
        /// </summary>
        private static void ConfigureMiddleware(WebApplication app)
        {
            // Must come first: Global exception handler
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Swagger UI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enforce HTTPS
            app.UseHttpsRedirection();

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // Add correlation ID for request tracking
            app.Use(async (context, next) =>
            {
                const string headerName = "X-Correlation-ID";

                if (!context.Request.Headers.TryGetValue(headerName, out var correlationId) ||
                    string.IsNullOrWhiteSpace(correlationId))
                {
                    correlationId = Guid.NewGuid().ToString();
                    context.Request.Headers[headerName] = correlationId;
                }

                context.Response.Headers[headerName] = correlationId!;
                await next();
            });

            // CORS
            app.UseCors("AllowSpecificOrigins");

            // Compression
            app.UseResponseCompression();

            // Rate Limiting
            app.UseRateLimiter();

            // Output Cache
            app.UseOutputCache();

            // Authentication → Authorization (order matters)
            app.UseAuthentication();
            app.UseAuthorization();

            // Map API controllers
            app.MapControllers();

            // Health checks endpoint
            app.MapHealthChecks("/health");
        }
    }
}
