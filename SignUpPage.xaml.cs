using System.Text.RegularExpressions;
using Supabase.Postgrest;
using SkiaSharp;
using MarketPlace.Services;
using Supabase;
namespace MarketPlace;

public partial class SignUpPage : ContentPage
{
    public string _avatarUrl;
    private string selectedLocation = string.Empty;
    private bool isPasswordVisible = true;
    private Timer _timer;
    private readonly Supabase.Client _supabaseClient;
    private const int MaxWidth = 500; // Maximum width for the compressed image
    private const int MaxHeight = 500; // Maximum height for the compressed image
    public byte[] _imageBytes;
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;

    public SignUpPage(AuthService authService, SupabaseService supabaseService)
    {
        InitializeComponent();
        _authService = authService;
        _supabaseService = supabaseService;
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        PasswordEntry.IsPassword = true;
    }
    // Event handler for button click to upload profile picture
    private async void OnUploadProfilePictureClicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;
        // Open file picker to select an image
        var result = await FilePicker.PickAsync(PickOptions.Images);

        if (result != null)
        {
            // Compress and display the selected image
            var fileStream = await result.OpenReadAsync();
            _imageBytes = await CompressImageAsync(fileStream);

            // Convert the byte array to an image and display it
            var imageSource = ImageSource.FromStream(() => new MemoryStream(_imageBytes));
            ProfileImage.Source = imageSource;

            LoadingOverlay.IsVisible = false;

        }
        LoadingOverlay.IsVisible = false;
    }
    // Compress the image
    private async Task<byte[]> CompressImageAsync(Stream imageStream)
    {
        // Load the image using SkiaSharp
        using var skiaImage = SKBitmap.Decode(imageStream);
        var ratio = Math.Min((float)MaxWidth / skiaImage.Width, (float)MaxHeight / skiaImage.Height);
        var newWidth = (int)(skiaImage.Width * ratio);
        var newHeight = (int)(skiaImage.Height * ratio);

        // Resize the image while keeping the aspect ratio
        var resizedImage = skiaImage.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);

        // Compress the image into a byte array
        using var memoryStream = new MemoryStream();
        resizedImage.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(memoryStream);
        return memoryStream.ToArray();
    }
    // Upload the compressed image to Supabase or other server
     

    private async void OnLocationButtonClicked(object sender, EventArgs e)
    {
        // Show a simple action sheet for location selection
        string location = await DisplayActionSheet("Select Your Location", "Cancel", null,
                                                   "KIU Ishaka", "Ishaka Town", "Bushenyi", "Mbarara","Kizinda","Lagos","Abuja","Bwegyi","Kabwohe","Not Listed");

        if (location != null && location != "Cancel")
        {
            selectedLocation = location;
            SelectedLocationLabel.Text = $"Location: {selectedLocation}";
            SelectedLocationLabel.TextColor = Colors.Black;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;
        await Task.Delay(4000);// Show loading
        string username = UsernameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim();
        string phone = PhoneEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();
        string confirmPassword = ConfirmPasswordEntry.Text?.Trim();
        selectedLocation = SelectedLocationLabel.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword) ||
            string.IsNullOrWhiteSpace(selectedLocation))
        {
            LoadingOverlay.IsVisible = false;  // Show loading
            await DisplayAlert("Error", "Please fill all fields and select a location.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", "Passwords do not match.", "OK");
            return;
        }

        if (password.Length < 6)
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", "Password must be at least 6 characters.", "OK");
            return;
        }
        if (EvaluatePasswordStrength(password) != "Very Strong")
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", "Password is too weak. Make it stronger by adding special characters, numbers and letters to continue.", "OK");
            return;
        }


        try
        {
            var QueryAnswer = await IsEmailAlreadyRegistered(email);
            if (QueryAnswer == true)
            {
                LoadingOverlay.IsVisible = false;
                await DisplayAlert("Error", "Email already registered. Please use a different email.", "OK");
                return;
            }

            var response = await _supabaseClient.Auth.SignUp(email, password, new Supabase.Gotrue.SignUpOptions
            {
                RedirectTo = "greendash://verify"
            });
            LoadingOverlay.IsVisible = false;
            bool answer = await DisplayAlert("Verify Email", "Check your email to verify your account and then click verified.", "VERIFIED", "CANCEL");
            if (answer)
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
               
                if (session.User != null && session.User.EmailConfirmedAt != null)
                {
                    if (_imageBytes != null)
                    {
                        var ImageId = Guid.NewGuid().ToString();
                        await _supabaseClient.Storage
                         .From("Images/Avatars")
                          .Upload(_imageBytes, ImageId);
                        _avatarUrl = _supabaseClient.Storage.From("Images").GetPublicUrl($"Avatars/{ImageId}");
                    }
                   
                     StoreUserData(username, email, phone, selectedLocation, _avatarUrl);

                }
                else
                {

                    await DisplayAlert("Error", "Email was not verified.", "Again");
                }

            }
            else
            {
                return;
            }





        }
        catch (Exception ex)
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", ex.Message, "OK");
        }

        // Example: Navigate to HomePage
        // await Navigation.PushAsync(new HomePage());
    }
    public async Task<bool> IsEmailAlreadyRegistered(string email)
    {
        try
        {
            // Query Supabase for the user with the given email
            var response = await _supabaseClient
                .From<User>()
                .Where(e => e.email == email)
                .Single();// Check for matching email       

            if (response != null)
            {
                return true;
            }
            // If response is not null, email already exists
            return false;
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., network issue, query failure)
            await DisplayAlert("Error", "An error occurred: " + ex.Message, "OK");
            return false;
        }
    }

    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        string password = e.NewTextValue;
        UpdatePasswordStrength(password);
    }

    private void UpdatePasswordStrength(string password)
    {
        string strength = EvaluatePasswordStrength(password);
        PasswordStrengthLabel.Text = $"Password Strength: {strength}";

        switch (strength)
        {
            case "Very Weak":
                PasswordStrengthLabel.TextColor = Colors.Red;
                break;
            case "Weak":
                PasswordStrengthLabel.TextColor = Colors.OrangeRed;
                break;
            case "Strong":
                PasswordStrengthLabel.TextColor = Colors.Orange;
                break;
            case "Very Strong":
                PasswordStrengthLabel.TextColor = Colors.Green;
                break;
            default:
                PasswordStrengthLabel.TextColor = Colors.Gray;
                break;
        }
    }

    private string EvaluatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Very Weak";

        int score = 0;
        if (password.Length >= 8) score++;
        if (Regex.IsMatch(password, @"\d")) score++; // Contains digit
        if (Regex.IsMatch(password, @"[a-z]")) score++; // Lowercase
        if (Regex.IsMatch(password, @"[A-Z]")) score++; // Uppercase
        if (Regex.IsMatch(password, @"[\W_]")) score++; // Special char

        return score switch
        {
            0 or 1 => "Very Weak",
            2 => "Weak",
            3 or 4 => "Strong",
            5 => "Very Strong",
            _ => "Very Weak"
        };
    }

    private void TogglePasswordVisibility_Clicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        PasswordEntry.IsPassword = !isPasswordVisible;
        TogglePasswordVisibility.Source = isPasswordVisible ? "make_invisible_icon.png" : "make_visible_icon.png";
    }
    private bool isConfirmVisible = false;
    private void ToggleConfirmVisibility_Clicked(object sender, EventArgs e)
    {
        isConfirmVisible = !isConfirmVisible;
        ConfirmPasswordEntry.IsPassword = !isConfirmVisible;
        ToggleConfirmVisibility.Source = isConfirmVisible ? "make_invisible_icon.png" : "make_visible_icon.png";
    }


    public async void StoreUserData(string username, string email, string phone, string location, string avatar)
    {
        LoadingOverlay.IsVisible = true;
        if (avatar == null)
        {
            avatar = "avatar.png";
        }
        Preferences.Set("UserEmail", email);
       
       
        
        var user = new User
        {
            username = username,
            email = email,
            phone = phone,
            location = location,
            avatar = avatar
        };
        
        var response = await _supabaseClient.From<User>().Insert(user); 
        if (response != null)
        {

            var user_email = _supabaseClient.Auth.CurrentUser.Email;
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Success", "You have been registered successfully", "Continue");
            await Navigation.PushAsync(new SignInPage(_authService,_supabaseService));

            //await Shell.Current.GoToAsync(nameof(SignInPage));

        }
        else
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("FAILED", "An error occured while trying to resgister you", "Go Back");
        }
        var wallet = new Wallet
        {
            user_email = email,
            balance = 0,
            id = Guid.NewGuid().ToString()
        };
        await _supabaseClient.From<Wallet>().Insert(wallet);
    }
    private async void NavigateToSignInPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage(_authService,_supabaseService));
        //await Shell.Current.GoToAsync(nameof(SignInPage));
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;
            var username = UsernameEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(username))
            {
                await DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            var session = await _authService.SignUp(email, password, username);
            if (session != null)
            {
                Preferences.Set("UserEmail", email);
                Preferences.Set("IsLoggedIn", true);
                await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage(_authService, _supabaseService));
    }
}
