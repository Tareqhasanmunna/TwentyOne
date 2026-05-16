using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using TwentyOne.BLL.Services.Implementations;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Implementations;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.Models;


namespace TwentyOne.Web.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // Coupon
            builder.Services.AddScoped<ICouponRepository, CouponRepository>();
            builder.Services.AddScoped<ICouponService, CouponService>();

            // Build the app
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

                await DatabaseSeeder.SeedRolesAsync(roleManager);
                await DatabaseSeeder.SeedSuperAdminAsync(userManager);
            }

            app.Run();
        }
    }
}