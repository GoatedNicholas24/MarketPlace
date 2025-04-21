using SkiaSharp;

namespace MarketPlace;

public partial class EditProfilePage : ContentPage
{
    public string _avatarUrl;
    public byte[] _imageBytes;
    private Supabase.Client _supabaseClient;
    private User _currentUser { get; set; } = new();
    private FileResult _newImage;
    public EditProfilePage()
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        _currentUser = new User();
        LoadProfile();
    }
    private async void LoadProfile()
    {
        var email = Preferences.Get("UserEmail", "null");
        var result = await _supabaseClient.From<User>().Where(x => x.email == email).Get();

        if (result.Models.Count > 0)
        {
            _currentUser = result.Models.First();
            UsernameEntry.Text = _currentUser.username;
            PhoneEntry.Text = _currentUser.phone;
            LocationEntry.Text = _currentUser.location;
            ProfileImage.Source = _currentUser.avatar;
        }
    }
    private async void OnChangePictureClicked(object sender, EventArgs e)
    {
        
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

             

        }
        
         
    }
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _currentUser.username = UsernameEntry.Text;
        _currentUser.phone = PhoneEntry.Text;
        _currentUser.location = LocationEntry.Text;

        if (_imageBytes != null)
        {
            // 📦 Upload to Supabase Storage (adjust according to your bucket settings)
            var ImageId = Guid.NewGuid().ToString();
            await _supabaseClient.Storage
             .From("Images/Avatars")
              .Upload(_imageBytes, ImageId);
            _avatarUrl = _supabaseClient.Storage.From("Images").GetPublicUrl($"Avatars/{ImageId}");
            _currentUser.avatar = _avatarUrl;
        }

        var model = await _supabaseClient
     .From<User>()
     .Where(x => x.email == _currentUser.email)
     .Single();
        model = _currentUser;
        await model.Update<User>();
        await _supabaseClient.From<Product>().Where(x => x.Email == _currentUser.email).Set(x => x.SellerAvatarUrl, _currentUser.avatar).Update();
        await DisplayAlert("Done", "Profile updated successfully.", "OK");
        await Navigation.PopAsync();
    }

    // ⏬ Resize/compress helper
    
    private const int MaxWidth = 500; // Maximum width for the compressed image
    private const int MaxHeight = 500; // Maximum height for the compressed image
  
    private async Task<byte[]> CompressImageAsync(Stream imageStream)
    {
        // Load the image using SkiaSharp
      
        using var skiaImage = SKBitmap.Decode(imageStream);
        if (skiaImage == null)
        { throw new Exception("Invalid image format or decode failed.");
            await DisplayAlert("Error", "Invalid image format","ok");
        }
            
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
    
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _supabaseClient.Auth.SignOut();
        Preferences.Remove("UserEmail");

        await Shell.Current.GoToAsync("//LoginPage");
    }

}