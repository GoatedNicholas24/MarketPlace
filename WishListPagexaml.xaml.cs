using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarketPlace.Models;
using MarketPlace.Services;

namespace MarketPlace
{
    public partial class WishListPagexaml : ContentPage
    {
        private readonly SupabaseService _supabaseService;
        private readonly AuthService _authService;

 
        private ObservableCollection<Product> wishlistItems { get; set; } = new();

        
        private bool isRefreshing;

        public WishListPagexaml(SupabaseService supabaseService, AuthService authService)
        {
            InitializeComponent();
            _supabaseService = supabaseService;
            _authService = authService;
            BindingContext = this;
            LoadWishlistItems();
        }

        private async void LoadWishlistItems()
        {
            try
            {
                isRefreshing = true;
                var userId = _authService.GetCurrentUser();
                if ( userId==null)
                {
                    await DisplayAlert("Error", "Please sign in to view your wishlist", "OK");
                    return;
                }

                var wishlist = await _supabaseService.GetWishlist(Preferences.Get("UserEmail",""));
                wishlistItems = new ObservableCollection<Product>(wishlist);
                WishlistItemsView.ItemsSource = wishlistItems;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load wishlist: {ex.Message}", "OK");
            }
            finally
            {
                isRefreshing = false;
            }
        }
        private async void RefreshWishList(object sender, EventArgs e)
        {
            try
            {
                
                 LoadWishlistItems();
            }
            finally
            {
                
            }
        }
        [RelayCommand]
        private async Task Refresh()
        {
              LoadWishlistItems();
        }

        [RelayCommand]
        private async Task AddToCart(Product product)
        {
            try
            {
                var userId = _authService.GetCurrentUser();
                if (userId == null)
                {
                    await DisplayAlert("Error", "Please sign in to add items to cart", "OK");
                    return;
                }

                await _supabaseService.AddToCart(Preferences.Get("UserEmail","null"), product.ProductId, 1);
                await DisplayAlert("Success", "Item added to cart", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add to cart: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task RemoveFromWishlist(Product product)
        {
            try
            {
                var userId = _authService.GetCurrentUser();
                if (userId == null)
                {
                    await DisplayAlert("Error", "Please sign in to remove items from wishlist", "OK");
                    return;
                }

                await _supabaseService.RemoveFromWishlist(Preferences.Get("UserEmail","null"), product.ProductId);
                wishlistItems.Remove(product);
                await DisplayAlert("Success", "Item removed from wishlist", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to remove from wishlist: {ex.Message}", "OK");
            }
        }
    }
}
