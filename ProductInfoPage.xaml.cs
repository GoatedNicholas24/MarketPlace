using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using MarketPlace.Services;
using Supabase.Interfaces;

namespace MarketPlace;
 
public partial class ProductInfoPage : ContentPage
{
    public string _productId;
    public Product _selectedProduct;
    public Supabase.Client _supabaseClient;
    public SupabaseService _supabaseService;
    public AuthService _authService;
     
    public ProductInfoPage(Product product)
    {
        InitializeComponent();
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };
       
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
        _supabaseService = new SupabaseService(_supabaseClient);
        _authService = new AuthService(_supabaseClient);
        _selectedProduct = product;
        ProductCategoryLabel.Text = product.Category;
        ProductDescriptionLabel.Text = product.Description;
        ProductImage.Source = product.Images;
        ProductPriceLabel.Text = product.Price.ToString();
        ProductNameLabel.Text = product.Name;
        if (product.SellerAvatarUrl != null) { SellerAvatar.Source = product.SellerAvatarUrl; }
        SellerNameLabel.Text = product.SellerName;
        
        
           
            
            

           
        }
    
   protected override void  OnAppearing()
    {
        base.OnAppearing();

        UpdatePopularityScore();
    }
    private async void UpdatePopularityScore()
    {
        Task.Delay(3000);
        var result = await _supabaseClient.From<Product>().Where(p => p.ProductId == _selectedProduct.ProductId).Get();
        int score = result.Model.PopularityScore;
        int newscore = score + 1;
        if (result == null)
        {
            return;
        }
        var response = await _supabaseClient.From<Product>().Where(p => p.ProductId == _selectedProduct.ProductId).Set(x => x.PopularityScore, newscore).Update();
    }
    
    private async void ViewSellerProfileButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Seller Profile", "Navigating to seller profile...", "OK");
        // Implement navigation to seller's profile page
    }

    private async void BuyNowButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CheckoutPage(_selectedProduct));
        //await Shell.Current.GoToAsync(nameof(CheckoutPage), new Dictionary<string, object> { { "productId", _selectedProduct.ProductId } });
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync(
    "Enter Quantity",
    "In what quantity should this item be added to your cart?",
    placeholder: "e.g. 1",
    maxLength: 3,
    keyboard: Keyboard.Numeric
);

        if (int.TryParse(result, out int quantity) && quantity > 0)
        {
            string useremail = Preferences.Get("UserEmail", "null"); // Retrieve logged-in user ID
            if (useremail == "null")
            {
                await DisplayAlert("Error", "You need to be logged in to add to wishlist.", "OK");
                return;
            }
            var cartId = Guid.NewGuid().ToString();
            var cartItem = new Cart
            {
                Id = cartId,
                User_Email = useremail,
                Product_Id = _selectedProduct.ProductId,
                Quantity = quantity.ToString() // Default quantity to 1
            };

            var response = await _supabaseClient.From<Cart>().Insert(cartItem);

            if (response != null)
            {
                await DisplayAlert("Success", "Product added to Cart!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to add to Cart.", "OK");
            }
            
        }
        else
        {
            await DisplayAlert("Invalid Input", "Please enter a valid number greater than 0.", "OK");
        }

       
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Just to be safe, clear the ProductId preference again
        if (Preferences.ContainsKey("SelectedProductId"))
        {
            Preferences.Remove("SelectedProductId");
        }

        // You can also do any other cleanup here if needed
        // e.g., canceling tasks, stopping timers, saving state, etc.
    }

    private async void NegotiatePriceButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Negotiate", "Starting price negotiation...", "OK");
        // Implement price negotiation feature
    }
    private async void GoBackButton_Clicked(object sender, EventArgs e)
    {
        //await Shell.Current.GoToAsync(nameof(HomePage1));
        // Navigate back
        await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
    }
    private async void OnAddToWishlistClicked(object sender, EventArgs e)
    {
        var userEmail = Preferences.Get("Useremail", "");
        var existing = await _supabaseClient
            .From<WishListItem>()
            .Where(w => w.Product_Id == _selectedProduct.ProductId && w.User_Email == userEmail)
            .Get();

        if (existing.Models.Count >=1)
        {
            Toast.Make("Product is already in your wishlist").Show();
        }
        else
        {
            string useremail = Preferences.Get("UserEmail", "null"); // Retrieve logged-in user ID
            if (useremail == "null")
            {
                await DisplayAlert("Error", "You need to be logged in to add to wishlist.", "OK");
                return;
            }
            var wishListId = Guid.NewGuid().ToString();
            var wishlistItem = new WishListItem
            {
                Id = wishListId,
                User_Email = useremail,
                Product_Id = _selectedProduct.ProductId
            };

            var response = await _supabaseClient.From<WishListItem>().Insert(wishlistItem);

            if (response != null)
            {
                await DisplayAlert("Success", "Product added to Wishlist!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to add to Wishlist.", "OK");
            }

        }

       
    }
     
 private async void NavigateToSellerProfile(object sender,EventArgs e)
    {
        await Navigation.PushAsync(new SellerProfile(_selectedProduct));
    }

}