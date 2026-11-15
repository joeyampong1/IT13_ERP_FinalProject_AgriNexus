using Microsoft.Extensions.Logging;
using AgriNexus.Services;
using Microsoft.EntityFrameworkCore;
using AgriNexus.DataLayer; 

namespace AgriNexus
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddScoped<AuthService>();

            // Register the Database Context with the connection string
            builder.Services.AddDbContext<AgriNexusDbContext>(options =>
                options.UseSqlServer("Server=LAPTOP-1NVCTOTP;Database=AgriNexus_DB;Trusted_Connection=True;TrustServerCertificate=True;"));

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}