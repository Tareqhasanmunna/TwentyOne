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

            // HttpClient pointing to our API
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            builder.Services.AddHttpClient<AuthApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            builder.Services.AddHttpClient<BrandApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            builder.Services.AddHttpClient<ProductApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            builder.Services.AddHttpClient<OrderApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            builder.Services.AddHttpClient<CouponApiService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
