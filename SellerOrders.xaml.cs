using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
 
using Supabase;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
 
using static Supabase.Postgrest.Constants;

namespace MarketPlace
{
    public partial class SellerOrders : ContentPage
    {
        private Supabase.Client _supabaseClient;
        public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();

        public SellerOrders()
        {
            InitializeComponent();
            BindingContext = this;
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,  // Ensures tokens refresh automatically
                AutoConnectRealtime = true
            };

            _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);


            LoadOrders();
        }

        private async void LoadOrders()
        {
            try
            {
                var email = Preferences.Get("UserEmail", "null");

                var response = await _supabaseClient
                    .From<Order>()
                    .Where(x => x.SellerEmail == email)
                    .Order(x => x.created_at, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                if (response.Models != null)
                {
                    Orders.Clear();
                    foreach (var order in response.Models)
                    {
                        Orders.Add(order);

                    }
                    OrdersCollectionView.ItemsSource = Orders;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load orders: " + ex.Message, "OK");
            }
        }

        // Accept Button Clicked
        public async void AcceptCommand(Order order)
        {
            order.status = "processing";
            await UpdateOrderStatus(order);
        }

        // Reject Button Clicked
        public async void RejectCommand(Order order)
        {
            order.status = "rejected";
            await UpdateOrderStatus(order);
        }

        // Delivered Button Clicked
        public async void DeliveredCommand(Order order)
        {
            order.status = "sent";
            await UpdateOrderStatus(order);
        }

        private async Task UpdateOrderStatus(Order order)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Order>()
                    .Where(x => x.reference == order.reference)
                    .Set(x => x.status, order.status)
                    .Update();

                await DisplayAlert("Success", $"Order marked as {order.status}", "OK");
                LoadOrders(); // Refresh list
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to update order: " + ex.Message, "OK");
            }
        }

        private void OnAcceptClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var order = button?.BindingContext as Order;
            if (order != null)
                AcceptCommand(order);
        }

        private void OnRejectClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var order = button?.BindingContext as Order;
            if (order != null)
                RejectCommand(order);
        }

        private void OnDeliveredClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var order = button?.BindingContext as Order;
            if (order != null)
                DeliveredCommand(order);
        }
        public async void OnChatClicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            // Get the BindingContext of the button's parent (the frame containing the order)
            var order = button?.BindingContext as Order;
            if (order != null)
            {
                var SellerEmail = order.SellerEmail; // The current logged-in user
                var BuyerEmail = order.buyer_email;

                // Check if a chat exists between the buyer and seller for this order
                var chat = await _supabaseClient.From<Chat>().Where(x => x.BuyerEmail == BuyerEmail && x.sellerEmail == SellerEmail).Get();

                if (chat != null && chat.Model != null)
                {
                    // Navigate to the existing chat
                    await Navigation.PushAsync(new ChatPage(chat.Model.id,chat.Model.sellerEmail,chat.Model.BuyerEmail));
                }
                else
                {
                    // Create a new chat
                    var NewChat = new Chat
                    {
                        id = Guid.NewGuid().ToString(),
                        BuyerEmail = BuyerEmail,
                        sellerEmail = SellerEmail,
                        
                    };
                    var newChat = await _supabaseClient.From<Chat>().Insert(NewChat);
                    await Navigation.PushAsync(new ChatPage(newChat.Model.id,newChat.Model.sellerEmail,newChat.Model.BuyerEmail));
                }
            }

        }


    }
}

