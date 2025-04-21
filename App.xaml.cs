using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel.Communication;
using Supabase;
using Supabase.Realtime;
using MarketPlace.Services;
using Supabase.Interfaces;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;

namespace MarketPlace
{
    public partial class App : Application
    {
        private readonly SupabaseService _supabaseService;
        private readonly AuthService _authService;
        private readonly INotificationManagerService _notificationService;
        private CancellationTokenSource _notificationCts;
        private bool _isNotificationLoopRunning;

        public App(SupabaseService supabaseService, AuthService authService, INotificationManagerService notificationService)
        {
            InitializeComponent();
            
            _supabaseService = supabaseService;
            _authService = authService;
            _notificationService = notificationService;
            
            // Set the app theme to match the system theme
            Application.Current.UserAppTheme = AppTheme.Unspecified;
            
            // Check if user is logged in
            CheckUserSession();
        }
        protected override Window CreateWindow(IActivationState activationState)
        {
            if (this.MainPage == null)
            {
                this.MainPage = new MainPage();
            }

            return base.CreateWindow(activationState);
        }


        private const string IsLoggedInKey = "isLoggedIn";
        private const string UserEmailKey = "UserEmail";
        private const string UserPasswordKey = "UserPassword";
        
        public static bool IsLoggedIn()
        {
            return Preferences.Get(IsLoggedInKey, false);
        }

        private async void CheckUserSession()
        {
            try
            {
                var session = await _authService.GetCurrentSession();
                bool isLoggedIn = IsLoggedIn();
                
                if (isLoggedIn && session != null)
                {
                    // If a user is signed in, navigate to the HomePage
                    MainPage = new NavigationPage(new HomePage1(_supabaseService, _authService));
                }
                else if (isLoggedIn && session == null)
                {
                    // Try to restore the session using stored credentials
                    var email = Preferences.Get(UserEmailKey, string.Empty);
                    var password = Preferences.Get(UserPasswordKey, string.Empty);
                    
                    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                    {
                        var newSession = await _authService.SignIn(email, password);
                        if (newSession != null)
                        {
                            MainPage = new NavigationPage(new HomePage1(_supabaseService, _authService));
                            return;
                        }
                    }
                    
                    // If we couldn't restore the session, clear the stored credentials
                    Preferences.Clear();
                    MainPage = new NavigationPage(new SignInPage(_authService, _supabaseService));
                }
                else
                {
                    // If no user is signed in, navigate to the SignInPage
                    MainPage = new NavigationPage(new SignInPage(_authService, _supabaseService));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user session: {ex.Message}");
                MainPage = new NavigationPage(new SignInPage(_authService, _supabaseService));
            }
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await InitializeNotificationsAsync();
        }

        private async Task InitializeNotificationsAsync()
        {
            try
            {
                await Task.Delay(1000); // Wait for the window to initialize (important)

                if (_notificationService != null && IsLoggedIn())
                {
                    await LoadMissedNotificationsAsync();
                    StartNotificationPolling();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing notifications: {ex.Message}");
            }
        }

        private void StartNotificationPolling()
        {
            if (_isNotificationLoopRunning) return;

            _notificationCts = new CancellationTokenSource();
            _isNotificationLoopRunning = true;

            Task.Run(async () =>
            {
                while (!_notificationCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await LoadMissedNotificationsAsync();
                        await Task.Delay(TimeSpan.FromSeconds(30), _notificationCts.Token); // Poll every 30 seconds
                    }
                    catch (OperationCanceledException)
                    {
                        // Polling was cancelled
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in notification polling: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(30), _notificationCts.Token); // Wait before retrying
                    }
                }
            }, _notificationCts.Token);
        }

        private async Task LoadMissedNotificationsAsync()
        {
            try
            {
                var email = Preferences.Get("UserEmail", "null");
                if (email == "null") return;

                var notifications = await _supabaseService.GetUnreadNotifications(email);
                foreach (var notification in notifications)
                {
                    _notificationService.SendNotification(notification.title, notification.content);
                    await _supabaseService.MarkNotificationAsRead(notification.id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading missed notifications: {ex.Message}");
            }
        }

        protected override async void OnResume()
        {
            base.OnResume();
            if (IsLoggedIn())
            {
                await LoadMissedNotificationsAsync();
                StartNotificationPolling();
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Stop the notification polling when app goes to background
            _notificationCts?.Cancel();
            _isNotificationLoopRunning = false;
        }
    }
}
