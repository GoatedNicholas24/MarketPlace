using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using MarketPlace.Services;
using Supabase;
using Microsoft.Extensions.DependencyInjection;
#if ANDROID
using MarketPlace.Platforms.Android;
#endif
namespace MarketPlace;

    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Supabase Client
            var supabaseUrl = "https://bbgpafulnowlgduckgie.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0";
            var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey);
            builder.Services.AddSingleton(supabaseClient);

            // Register Services
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<SupabaseService>();

#if ANDROID
            builder.Services.AddTransient<INotificationManagerService, MarketPlace.Platforms.Android.NotificationManagerService>();
#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            Services = app.Services;
            return app;
        }

        public static IServiceProvider Services { get; private set; }
    }

