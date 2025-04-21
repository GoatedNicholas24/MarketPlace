using Supabase.Gotrue.Mfa;
using Supabase.Interfaces;

namespace MarketPlace;

public partial class SellerRegistrationPage : ContentPage
{
    
    public Supabase.Client _supabaseClient;
    public SellerRegistrationPage()
	{
		InitializeComponent();
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
        BindingContext = new SellerRegistrationViewModel();
    }
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        string location = LocationPicker.SelectedItem?.ToString() + ":" + PhysicalAddressEntry.Text;
        var email = Preferences.Get("UserEmail", "example@gmail.com");
        // Validate required fields
        if (string.IsNullOrWhiteSpace(FullnameEntry.Text) ||
            string.IsNullOrWhiteSpace(PrimaryPhoneEntry.Text) ||
            string.IsNullOrWhiteSpace(BusinessNameEntry.Text) ||
            BusinessTypeEntry.SelectedItem == null ||
            string.IsNullOrWhiteSpace(PhysicalAddressEntry.Text) ||
            ItemCategory.SelectedItem == null ||
            string.IsNullOrWhiteSpace(BusinessDescriptionEditor.Text)||
             string.IsNullOrWhiteSpace(PINEntry.Text)) 
        {
            await DisplayAlert("Error", "Please fill in all required fields before proceeding.", "OK");
            return;
        }
        if(capturedLatitude == null || capturedLongitude == null)
        {
            await DisplayAlert("Title", "Please capture your location before proceeding.", "OK");
        }
        if (!TermsCheckBox.IsChecked)
        {
            await DisplayAlert("Alert!", "Please read through and agree with terms and conditions by checking the box.", "OK");
            return;
        }
        var sellerData = new Seller
        {
            Name = FullnameEntry.Text,
            Email = email,
            PrimaryPhone = PrimaryPhoneEntry.Text,
            SecondaryPhone = SecondaryPhoneEntry.Text,
            BusinessName = BusinessNameEntry.Text,
            BusinessType = BusinessTypeEntry.SelectedItem?.ToString(),
            Location = location,
            ItemsSold = ItemCategory.SelectedItem?.ToString(),
            BusinessDescription = BusinessDescriptionEditor.Text,
            Approved = false, // Admin will verify manually
            Latitude = capturedLatitude,
            Longitude = capturedLongitude,
            WalletPin = PINEntry.Text
        };

        try
        {
            var response = await _supabaseClient.From<Seller>().Insert(sellerData);
            if (response==null)
            {
                await DisplayAlert("Error", "Failed to register seller: " + response.ResponseMessage, "OK");
                return;
            }
            
            await DisplayAlert("Success", "Seller registration submitted! Awaiting approval.", "OK");
            await Navigation.PopAsync();
            //await Shell.Current.GoToAsync(nameof(MyShopPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to register: " + ex.Message, "OK");
        }
    }
    double capturedLatitude;
    double capturedLongitude;

    private async void OnCaptureLocationClicked(object sender, EventArgs e)
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                capturedLatitude = location.Latitude;
                capturedLongitude = location.Longitude;

                LatitudeLabel.Text = $"Latitude: {capturedLatitude}";
                LongitudeLabel.Text = $"Longitude: {capturedLongitude}";
            }
            else
            {
                await DisplayAlert("Error", "Could not get location", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Location Error", ex.Message, "OK");
        }
    }

}