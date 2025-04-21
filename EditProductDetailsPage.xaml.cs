using Supabase.Interfaces;

namespace MarketPlace;

public partial class EditProductDetailsPage : ContentPage
{
    public string ProductName;
    public string ProductCartegory;
    public string ProductDescription;
    public decimal ProductPrice;
    public bool ProductNegotiable;
    public string ProductDeliveryOption;
    public Product product;
    public string _useremail;
    public string _imagePath;
    public Supabase.Client _supabaseClient;
    public EditProductDetailsPage(string productId)
	{
		InitializeComponent();
        EditProductFormUi(productId);
    }
    private async void ClosePopupButton_Clicked(object sender, EventArgs e)
    {
        EditProductPopup.IsVisible = false;
    }
    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = FilePickerFileType.Images,
            PickerTitle = "Select a product image"
        });

        if (result != null)
        {
            _imagePath = result.FullPath;
            ProductImagePreview.Source = ImageSource.FromFile(_imagePath);
        }

    }
    private async void SaveButton_Clicked(object sender, EventArgs e)
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

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(price))
        {
            LoadingOverlay.IsVisible = false;
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }

        string imageUrl = string.Empty;
        if (!string.IsNullOrEmpty(_imagePath))
        {
            var fileBytes = File.ReadAllBytes(_imagePath);
            var ImageId = Guid.NewGuid().ToString();
            var result = await _supabaseClient.From<Product>().Where(p => p.ProductId == productId)
               .Get();
            var retProduct = result.Model;
            var imageurl = retProduct.Images;
            var imageDeleteResponse = await _supabaseClient.Storage.From("Images/Products").Remove(new List<string> { imageurl });
            var uploadResult = await _supabaseClient.Storage.From("Images/Products").Upload(fileBytes, ImageId);

            if (uploadResult != null)
            {
                // Generate a  url
                var publicUrl = _supabaseClient.Storage.From("Images/Products").GetPublicUrl(ImageId);

                imageUrl = publicUrl;
            }

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
            Images = imageUrl,

        };


        UpdateProduct(newProduct);
        LoadingOverlay.IsVisible = false;
        await DisplayAlert("Success", "Product uploaded successfully!", "OK");


        ClearForm();


    }
    private async void EditProductFormUi(string productId)
    {
        var response = await _supabaseClient
               .From<Product>()
               .Where(p => p.ProductId == productId)
               .Get();
        if (response.Models.Count > 0)
        {
            product = response.Model;


            ProductName = product.Name;
            ProductDescription = product.Description;
            ProductDeliveryOption = product.DeliveryOption;
            ProductNegotiable = product.Negotiable;
            ProductPrice = product.Price;
            ProductCartegory = product.Category;



        }
        EditProductPopup.IsVisible = true;
        ProductNameEntry.Text = product.Name;
        ProductNameEntry.Text = product.Name;
        DescriptionEditor.Text = product.Description;
        PriceEntry.Text = product.Price.ToString();
        ProductImagePreview.Source = product.Images;
    }

       
    
    private async void UpdateProduct(Product product)
    {
        /*string name = ProductNameEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();
        string description = DescriptionEditor.Text?.Trim();
        string price = PriceEntry.Text;
        bool isNegotiable = NegotiableSwitch.IsToggled;
        bool pickup = PickupCheckBox.IsChecked;
        bool delivery = DeliveryCheckBox.IsChecked;*/
        var Id = product.ProductId;
        try
        {
            if (product.Name != ProductName)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Name, product.Name).Update();
            }
            if (product.Category != ProductCartegory)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Category, product.Category).Update();
            }
            if (product.Description != ProductDescription)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Description, product.Description).Update();
            }
            if (product.Price != ProductPrice)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Price, product.Price).Update();
            }
            if (product.Negotiable != ProductNegotiable)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Negotiable, product.Negotiable).Update();
            }
            if (product.DeliveryOption != ProductDeliveryOption)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.DeliveryOption, product.DeliveryOption).Update();
            }
            if (product.Images != null)
            {
                await _supabaseClient.From<Product>().Where(x => x.ProductId == Id).Set(x => x.Images, product.Images).Update();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("ERROR", ex.Message, "ok");
        }
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
}