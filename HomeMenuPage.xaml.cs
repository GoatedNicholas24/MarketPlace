using Microsoft.Extensions.DependencyInjection;
using Supabase.Interfaces;
using MarketPlace.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
namespace MarketPlace;

public partial class HomeMenuPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    public Supabase.Client _supabaseClient;
    private ObservableCollection<MenuItem> _menuItems;
    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public HomeMenuPage(AuthService authService, SupabaseService supabaseService)
    {
        try
        {
            if (supabaseService == null)
            {
                throw new ArgumentNullException(nameof(supabaseService));
            }
            if (authService == null)
            {
                throw new ArgumentNullException(nameof(authService));
            }

            InitializeComponent();
            _authService = authService;
            _supabaseService = supabaseService;
        }
        catch (Exception ex)
        {
            // Log the error and show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Error initializing HomeMenuPage: {ex.Message}");
            DisplayAlert("Error", "There was a problem initializing the page. Please try again later.", "OK");
        }
    }
      
 

    private async void CartTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new CartPage(_authService, _supabaseService));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to CartPage: {ex.Message}");
            await DisplayAlert("Error", "There was a problem opening the cart. Please try again later.", "OK");
        }
    }

    private async void WishlistTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new WishListPagexaml(_supabaseService,_authService));
    }

    private async void OrdersTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrderHistoryPage(_supabaseService,_authService));
    }

    private async void ChatTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChatListPage(_authService, _supabaseService));
    }

    private async void MyShopTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyShopPage(_authService, _supabaseService));
    }

    private async void SettingsTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Settings());
    }

    private async void LogOutTapped(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Log Out", "Are you sure you want to log out?", "Yes", "No");
        if (confirm)
        {
            try
            {
                // Sign out from Supabase
                await _authService.SignOut();
                
                // Clear all stored credentials
                Preferences.Clear();
                
                // Navigate to sign in page
                await Navigation.PushAsync(new SignInPage(_authService, _supabaseService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to log out: " + ex.Message, "OK");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
 