using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarketPlace.Models;
using MarketPlace.Services;

namespace MarketPlace
{
    public partial class OrderHistoryPage : ContentPage
    {
        private readonly SupabaseService _supabaseService;
        private readonly AuthService _authService;

        
        private ObservableCollection<Order> Orders { get; set; } = new();

        
        private bool isRefreshing;

        public OrderHistoryPage(SupabaseService supabaseService, AuthService authService)
        {
            InitializeComponent();
            _supabaseService = supabaseService;
            _authService = authService;
            BindingContext = this;
            LoadOrders();
        }
        private async void RefreshOrders(object sender, EventArgs e)
        {
            LoadOrders();
        }
        private async Task LoadOrders()
        {
            try
            {
                isRefreshing = true;
                var userId = _authService.GetCurrentUser();
                if (userId == null)
                {
                    await DisplayAlert("Error", "Please sign in to view your orders", "OK");
                    return;
                }

                var response = await _supabaseService.GetOrders(Preferences.Get("UserEmail","null"));
                Orders = new ObservableCollection<Order>(response);
                OrdersView.ItemsSource = Orders;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load orders: {ex.Message}", "OK");
            }
            finally
            {
                isRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadOrders();
        }

        [RelayCommand]
        private async Task Chat(Order order)
        {
            try
            {
                if (order == null) return;

                var chat = await _supabaseService.GetOrCreateChat(order.buyer_email, order.SellerEmail);
                await Navigation.PushAsync(new ChatPage(chat.id, order.buyer_email, order.SellerEmail));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to start chat: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Cancel(Order order)
        {
            try
            {
                if (order == null) return;

                var result = await DisplayAlert("Confirm", "Are you sure you want to cancel this order?", "Yes", "No");
                if (!result) return;

                await _supabaseService.CancelOrder(order.reference);
                await LoadOrders();
                await DisplayAlert("Success", "Order cancelled successfully", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to cancel order: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Confirm(Order order)
        {
            try
            {
                if (order == null) return;

                var result = await DisplayAlert("Confirm", "Have you received this order?", "Yes", "No");
                if (!result) return;

                await _supabaseService.ConfirmOrder(order.reference);
                await LoadOrders();
                await DisplayAlert("Success", "Order confirmed successfully", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to confirm order: {ex.Message}", "OK");
            }
        }
    }
}