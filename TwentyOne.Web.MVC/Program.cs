using TwentyOne.Web.MVC.Services;

namespace TwentyOne.Web.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Set the public API base URL from configuration
            ApiConfig.PublicBaseUrl = builder.Configuration
                    ["ApiSettings:PublicBaseUrl"]
                    ?? "http://localhost:5300";

            // HttpClient pointing to our API
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            // HttpClient with SSL bypass for development
            void ConfigureClient(HttpClient client) =>
                client.BaseAddress = new Uri(apiBaseUrl!);

            builder.Services.AddHttpClient<AuthApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<BrandApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<ProductApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<OrderApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<CouponApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<ImageApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<BannerApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient<PreOrderApiService>(ConfigureClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                     ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.SecurePolicy =
                    CookieSecurePolicy.SameAsRequest;
                options.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append(
                    "X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append(
                    "X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Append(
                    "X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append(
                    "Referrer-Policy",
                    "strict-origin-when-cross-origin");
                context.Response.Headers.Append(
                    "Permissions-Policy",
                    "camera=(), microphone=(), geolocation=()");
                // CSP for MVC
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' " +
                    "cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' " +
                    "cdn.jsdelivr.net; " +
                    "img-src 'self' data: http://localhost:5300 https://localhost:7200 https://*.trycloudflare.com; " + 
                    "font-src 'self' cdn.jsdelivr.net;");
                await next();
            });

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "product",
                pattern: "Home/Product/{slug}",
                defaults: new { controller = "Home", action = "Product" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //.WithStaticAssets()

            app.MapControllerRoute(
                name: "admin",
                pattern: "Admin/{action=Login}/{id?}",
                defaults: new { controller = "Admin" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
