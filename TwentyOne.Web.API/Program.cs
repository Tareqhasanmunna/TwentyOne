using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using TwentyOne.BLL.Helpers;
using TwentyOne.BLL.Services.Implementations;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Implementations;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.Models;
using TwentyOne.Web.API.Services;


namespace TwentyOne.Web.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override(
                    "Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.Hosting.Lifetime",
                    Serilog.Events.LogEventLevel.Information)
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/twentyone-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
                        "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Use Serilog for logging
            builder.Host.UseSerilog();

            // 1. Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration
                    .GetConnectionString("DefaultConnection")));

            // 2. ASP.NET Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 3. JWT Authentication
            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>();

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            builder.Services.AddAuthorization();

            // 4. Controllers
            builder.Services.AddControllers();

            // 5. OpenAPI
            builder.Services.AddOpenApi();

            // 6. Register services
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Brand
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IBrandService, BrandService>();

            // Product
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            // Order
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<OrderPdfService>();

            // Coupon
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<ICouponService, CouponService>();

            //Image Upload
            builder.Services.AddScoped<ImageUploadService>();

            // Banner & SiteSettings
            builder.Services.AddScoped<IBannerRepository, BannerRepository>();
            builder.Services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("StrictCors", policy =>
                {
                    policy
                        // Only allow MVC frontend
                        .WithOrigins(
                            "https://localhost:7155",
                            "http://localhost:5155",
                            // Add production domain here later
                            "https://www.twentyone.com.bd",
                            "https://twentyone.com.bd")
                        .WithMethods("GET", "POST", "PUT", "DELETE")
                        .WithHeaders(
                            "Content-Type",
                            "Authorization",
                            "X-Requested-With")
                        .AllowCredentials();
                });

                // Separate open policy for public endpoints
                // (search, products, banners)
                options.AddPolicy("PublicCors", policy =>
                {
                    policy.AllowAnyOrigin()
                          .WithMethods("GET")
                          .AllowAnyHeader();
                });
            });

            // PreOrder
            builder.Services.AddScoped<IPreOrderRepository, PreOrderRepository>();
            builder.Services.AddScoped<IPreOrderService, PreOrderService>();

            // Cache
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CacheService>();

            // Rate limiting
            builder.Services.AddRateLimiter(options =>
            {
                // Auth endpoints — max 5 requests per minute
                options.AddFixedWindowLimiter("auth", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 2;
                });

                // General API — max 60 requests per minute
                options.AddFixedWindowLimiter("general", opt =>
                {
                    opt.PermitLimit = 60;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 10;
                });

                options.RejectionStatusCode = 429;
            });

            // Sanitization service
            builder.Services.AddScoped<SanitizationService>();

            // Build the app
            var app = builder.Build();
            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowMVC");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Security headers middleware
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append(
                    "X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append(
                    "X-Frame-Options", "DENY");
                context.Response.Headers.Append(
                    "X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append(
                    "Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append(
                    "Permissions-Policy",
                    "camera=(), microphone=(), geolocation=()");
                await next();
            });
            app.UseCors("StrictCors");

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();
                var siteSettings = scope.ServiceProvider
                .GetRequiredService<ISiteSettingRepository>();

                await DatabaseSeeder.SeedSiteSettingsAsync(siteSettings);
                await DatabaseSeeder.SeedRolesAsync(roleManager);
                await DatabaseSeeder.SeedSuperAdminAsync(userManager);
                await DatabaseSeeder.SeedGuestAccountAsync(userManager);
            }

            app.Run();
        }
    }
}