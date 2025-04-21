using MarketPlace.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Supabase;

namespace MarketPlace;

public partial class SignInPage : ContentPage
{
    private bool isPasswordVisible = true;
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;

    public SignInPage(AuthService authService, SupabaseService supabaseService)
    {
        InitializeComponent();
        _authService = authService;
        _supabaseService = supabaseService;
        PasswordEntry.IsPassword = true;
    }
   
    // SignIn Button Clicked
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter both email and password", "OK");
                return;
            }

            var session = await _authService.SignIn(email, password);
            if (session != null)
            {
                // Store credentials for future automatic login
                Preferences.Set("UserEmail", email);
                Preferences.Set("UserPassword", password);
                Preferences.Set("IsLoggedIn", true);
                Preferences.Set("UserEmail", email);
                await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
     

    private void OnTogglePasswordVisibilityClicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;

        PasswordEntry.IsPassword = !isPasswordVisible;
        ToggleVisibilityButton.Source = isPasswordVisible ? "make_invisible_icon.png" : "make_visible_icon.png";
    }
    private const string IsLoggedInKey = "isLoggedIn";

    public static void SetLoggedInState(bool isLoggedIn)
    {
        Preferences.Set(IsLoggedInKey, isLoggedIn);
    }

    

    // Redirect to SignUp Page
    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage(_authService, _supabaseService));
    }
    
}
