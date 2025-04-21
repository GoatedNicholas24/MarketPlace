using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace MarketPlace;

public partial class AddProductPage : ContentPage
{
    private const int MaxWidth = 500; // Maximum width for the compressed image
    private const int MaxHeight = 500;
    private Supabase.Client _supabaseClient;
    private string _imagePath = string.Empty;
    public Stream _imageStream;
    private ObservableCollection<Product> previewModel { get; set; } = new();
    public AddProductPage()
	{
		InitializeComponent();
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
    }
    // Handle Image Selection
    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = FilePickerFileType.Images,
            PickerTitle = "Select a product image"
        });
         _imageStream = await result.OpenReadAsync();


        if (result != null)
        {
            _imagePath = result.FullPath;
            ProductImagePreview.Source = ImageSource.FromFile(_imagePath);
        }
        previewModel.Clear();
    }
    // Handle Product Upload
     
    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;
        string name = ProductNameEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();
        string description = DescriptionEditor.Text?.Trim();
        string price = PriceEntry.Text;
        bool isNegotiable = NegotiableSwitch.IsToggled;
        bool pickup = PickupCheckBox.IsChecked;
        bool delivery = DeliveryCheckBox.IsChecked;
        bool Both = BothCheckBox.IsChecked;
        var productId = Guid.NewGuid().ToString();
        var deliveryOption = "Delivery";
        if (Both)
        {
            deliveryOption = "Delivery and Pickup";
        }
        if (delivery)
        {
            deliveryOption = "Delivery Only";
        }
        if (pickup)
        {
            deliveryOption = "Pick up Only";
        }

        string sellerEmail = Preferences.Get("UserEmail", "null");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(price) )
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        string imageUrl = string.Empty;
        if (!string.IsNullOrEmpty(_imagePath))
        {
            var fileBytes = await CompressImageAsync(_imageStream);
            var ImageId = Guid.NewGuid().ToString(); 
            var uploadResult = await _supabaseClient.Storage.From("Images/Products").Upload(fileBytes, ImageId);
 
            if (uploadResult != null)
            {
                // Generate a  url
                var publicUrl = _supabaseClient.Storage.From("Images/Products").GetPublicUrl(ImageId);
                
                imageUrl = publicUrl; 
            }
           
        }
        string useremail = Preferences.Get("UserEmail", "null");
        var sellerResponse = await _supabaseClient.From<User>().Where(e => e.email == useremail).Get();
        string avatarUrl = sellerResponse.Model.avatar;
        string seller = sellerResponse.Model.username;
        var newProduct = new Product
        {
            Name = name,
            ProductId = productId,
            Category = category,
            Description = description,
            Price = Convert.ToDecimal(price),
            Negotiable = isNegotiable,
            DeliveryOption = deliveryOption,
            Email = sellerEmail,
            Images = imageUrl,
            SellerAvatarUrl = avatarUrl,
            SellerName = seller

        };


        var response = await _supabaseClient.From<Product>().Insert(newProduct);
        if (response!=null)
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Success", "Product uploaded successfully!", "OK");
           /* var result = await _supabaseClient.From<Product>().Select("*").Get();
            if (result != null)
            {
                var resultModels = result.Models;
                if(resultModels.Count > 0)
                {
                    foreach(var model in resultModels)
                    {
                        if(model.ProductId == productId)
                        {
                            previewModel.Add(model);
                        }
                    }
                }
            }*/
            
            ClearForm();
        }
        else
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", response.ResponseMessage.ToString(), "OK");
        }
    }
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
    private void ClearForm()
    {
        ProductNameEntry.Text = string.Empty;
        CategoryPicker.SelectedItem = null;
        DescriptionEditor.Text = string.Empty;
        PriceEntry.Text = string.Empty;
        NegotiableSwitch.IsToggled = false;
        PickupCheckBox.IsChecked = false;
        DeliveryCheckBox.IsChecked = false;
        ProductImagePreview.Source = null;
        _imagePath = string.Empty;
    }

    private async void PreviewButton_Clicked(object sender, EventArgs e)
    {
        string name = ProductNameEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();
        string description = DescriptionEditor.Text?.Trim();
        string price = PriceEntry.Text;
        bool isNegotiable = NegotiableSwitch.IsToggled;
        bool pickup = PickupCheckBox.IsChecked;
        bool delivery = DeliveryCheckBox.IsChecked;
        bool Both = BothCheckBox.IsChecked;
        var productId = new Guid().ToString();
        var deliveryOption = "Delivery";
        if (Both)
        {
            deliveryOption = "Delivery and Pickup";
        }
        if (delivery)
        {
            deliveryOption = "Delivery Only";
        }
        if (pickup)
        {
            deliveryOption = "Pick up Only";
        }

        string sellerEmail = Preferences.Get("UserEmail", "null");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(price))
        {
            DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        

        var newProduct = new Product
        {
            Name = name,
            ProductId = productId,
            Category = category,
            Description = description,
            Price = Convert.ToDecimal(price),
            Negotiable = isNegotiable,
            DeliveryOption = deliveryOption,
            Email = sellerEmail,
            Images = _imagePath,

        };
        previewModel.Add(newProduct);
        await Navigation.PushAsync(new ProductPreviewPage(newProduct));
        
    }
}

