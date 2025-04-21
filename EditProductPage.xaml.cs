using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using Supabase.Interfaces;

namespace MarketPlace;

public partial class EditProductPage : ContentPage
{
    /*string name = ProductNameEntry.Text?.Trim();
        string category = CategoryPicker.SelectedItem?.ToString();
        string description = DescriptionEditor.Text?.Trim();
        string price = PriceEntry.Text;
        bool isNegotiable = NegotiableSwitch.IsToggled;
        bool pickup = PickupCheckBox.IsChecked;
        bool delivery = DeliveryCheckBox.IsChecked;*/
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
    public ObservableCollection<Product> SellerProducts { get; set; } = new();
    public ObservableCollection<Product> SellerProduct { get; set; } = new();
    public EditProductPage()
    {
        InitializeComponent();
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);

        _useremail = Preferences.Get("UserEmail", "null");
        LoadProducts();
      
    }
    private async void LoadProducts()
    {
        var response = await _supabaseClient.From<Product>().Select("*").Get();
        if (response.Models.Count == 0)
        {
            await DisplayAlert("Error", "No products fetched", "Ok");
        }
        var productList = response.Models;

        SellerProducts.Clear();
        foreach (var product in productList)
        {
            if (product.Email == _useremail)
            {
                SellerProducts.Add(product);
            }


        }
        ProductsView.ItemsSource = SellerProducts;

    }
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Get the Button that was clicked
        Button button = sender as Button;

        // Retrieve the product ID from the CommandParameter
        var product = button?.BindingContext as Product;
        
            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to delete this product?", "Yes", "No");
            if (!confirm)
                return;


        var imageurl = product.Images;
            var imageDeleteResponse = await _supabaseClient.Storage.From("Images/Products").Remove( imageurl );
            var response = _supabaseClient
                .From<Product>()
                .Where(p => p.ProductId == product.ProductId)
                .Delete();

            if (response != null)
            {
                await DisplayAlert("Success", "Product deleted successfully!", "OK");

                // Refresh the UI to reflect the deleted product
                LoadProducts();
            }
            else
            {
                await DisplayAlert("Error", response.ToString(), "OK");
            }
         
    }
    private async void OnEditButtonClicked(object sender, EventArgs e)
    {


        if (sender is Button button && button.CommandParameter is string productId)
        {
            // Navigate to the EditProductPage with the selected product ID
            //await Navigation.PushAsync(new EditProductDetailsPage(productId));
            // await Shell.Current.GoToAsync(nameof(EditProductDetailsPage));
            await Navigation.PushAsync(new EditProductDetailsPage(productId));
        }



    }
   
    // Handle Image Selection
  
   
    private async void SubmitButton_Clicked(object sender, EventArgs e)
    {
       
    }
 

    
}