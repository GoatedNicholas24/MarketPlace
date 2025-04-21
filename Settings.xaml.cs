namespace MarketPlace;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
	}
     
       

        // Navigate to Edit Profile Page
        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditProfilePage());
        }

        // Navigate to Change Password Page
        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChangePasswordPage());
        }
    private async void OnDeleteAccountClicked(object sender, EventArgs e)
    {

    }
    private async void OnChangeThemeClicked(object sender, EventArgs e)
    {
        string selected = await DisplayActionSheet("Choose Theme", "Cancel", null, "Bright", "Dark");

        if (selected == "Bright")
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            Preferences.Set("AppTheme", "Light");
        }
        else if (selected == "Dark")
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            Preferences.Set("AppTheme", "Dark");
        }
    }

    // Open Return and Refund Policy Page or display policy
    private async void OnPolicyClicked(object sender, EventArgs e)
        {
        await Navigation.PushAsync(new ReturnAndRefundPolicyPage());
        //await DisplayAlert("Return and Refund Policy", "This feature is unavailable.Comming Soon", "OK");
        }

        // Open Theme Settings (e.g., dark mode, light mode)
       

        // Navigate to Feedback/Rate App
        private async void OnFeedbackClicked(object sender, EventArgs e)
        {
       await Navigation.PushAsync(new RatingAndFeedBackPage());
            //await DisplayAlert("Feedback",  "This feature is unavailable.Comming Soon", "OK");
        }

        // Contact Support
        private async void OnContactSupportClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Contact Support", "This feature is unavailable.Comming Soon", "OK");
        }

        // Report a bug
        private async void OnReportBugClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Report Bug","This feature is unavailable.Comming Soon", "OK");
        }
    }
