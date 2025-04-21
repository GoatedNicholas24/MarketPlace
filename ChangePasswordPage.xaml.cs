using Supabase.Gotrue;
using Supabase.Interfaces;

namespace MarketPlace;

public partial class ChangePasswordPage : ContentPage
{
    
    public Supabase.Client _supabaseClient;
	public ChangePasswordPage()
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");

    }
    private async void OnUpdatePasswordClicked(object sender, EventArgs e)
    {

        string email = Preferences.Get("UserEmail", "null");
        string currentPassword = CurrentPasswordEntry.Text?.Trim();
        string newPassword = NewPasswordEntry.Text?.Trim();
        string confirmPassword = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlert("Error", "All fields are required.", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        try
        {
            var session = await _supabaseClient.Auth.SignIn(email, currentPassword);
            if (session != null && session.User != null)
            {
                await _supabaseClient.Auth.Update(new UserAttributes
                {
                    Password = newPassword
                });

                await DisplayAlert("Success", "Password changed successfully!", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to change password: {ex.Message}", "OK");
        }
    }
}