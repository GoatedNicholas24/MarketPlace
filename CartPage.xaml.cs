using Supabase.Interfaces;
using System.Collections.ObjectModel;
using System.Formats.Tar;
using MarketPlace.Services;
using Supabase.Postgrest.Models;
using System.Windows.Input;

namespace MarketPlace;

public partial class CartPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
   
    private bool _isRefreshing;

    public ObservableCollection<Product> CartProducts { get; set; } = new();
   

    public ICommand RefreshCommand => new Command(async () =>
    {
        
        await LoadCartAsync();
         
    });

    public CartPage(AuthService authService, SupabaseService supabaseService)
    {
        InitializeComponent();
        if (supabaseService == null)
        {
            throw new ArgumentNullException(nameof(supabaseService));
        }
        if (authService == null)
        {
            throw new ArgumentNullException(nameof(authService));
        }
        
        _authService = authService;
        _supabaseService = supabaseService;
       
        BindingContext = this;
        LoadCartAsync();
    }

    private async void RefreshCart(object sender, EventArgs e)
    {
       // IsRefreshing = true;
        await LoadCartAsync();
       // IsRefreshing = false;
    }
    private async Task LoadCartAsync()
    {
        string userEmail = Preferences.Get("UserEmail", "");

        var cartEntries = await _supabaseService.GetCartItems(userEmail);

        if (cartEntries.Count == 0)
            return;
       CartProducts.Clear();

        // 2. Load the product details and attach quantity
        foreach (var cartEntry in cartEntries)
        {
            var product = await _supabaseService.GetProductById(cartEntry.Product_Id);
            if (product != null)
            {
                if (int.TryParse(cartEntry.Quantity, out int quantity))
                {
                    product.Quantity = quantity;
                    CartProducts.Add(product);
                }
            }
        }

        CartCollectionView.ItemsSource = CartProducts;
        CalculateTotalAmount();
    }

    private int quantity = 1;
    private void CalculateTotalAmount()
    {
        decimal total = 0;

        foreach (var item in CartProducts)
        {
            total += item.Price * item.Quantity;
        }

        TotalAmountLabel.Text = $"Total: UGX {total:N0}";
    }

    // Navigate to Checkout Page
    private async void OrderAllClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartCheckoutPage(CartProducts.ToList()));
    }

    public async Task RemoveFromCartAsync(Product product)
    {
        try
        {
            var success = await _supabaseService.RemoveFromCart(product.ProductId);
            if (success)
            {
                CartProducts.Remove(product);
                await DisplayAlert("Removed", "Product removed from cart.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Could not remove product from cart.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    public async void RemoveFromCartClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var product = button?.BindingContext as Product;

        if (product == null)
        {
            return;
        }
        await RemoveFromCartAsync(product);
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_authService, _supabaseService));
    }

    private async void ChatIcon_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChatListPage(_authService,_supabaseService));
    }

    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage(_authService,_supabaseService));
    }

    public async void HomeMenuButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomeMenuPage(_authService,_supabaseService));
    }

    private void IncreaseQuantityClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var product = button?.BindingContext as Product;
        if (product != null)
        {
            product.Quantity++;
            CalculateTotalAmount();
        }
    }

    private void DecreaseQuantityClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var product = button?.BindingContext as Product;
        if (product != null)
        {
            if (product.Quantity > 1)
            {
                product.Quantity--;
                CalculateTotalAmount();
            }
            else
            {
                DisplayAlert("Limit", "Minimum quantity is 1.", "OK");
            }
        }
    }
}