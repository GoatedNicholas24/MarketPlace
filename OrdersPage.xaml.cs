using System.Collections.ObjectModel;
using Supabase.Interfaces;

namespace MarketPlace {
    public partial class OrdersPage : ContentPage
    {
        public ObservableCollection<Order> Orders;
        public Supabase.Client _supabaseClient;
        public OrdersPage()
        {
            InitializeComponent();
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
            var sellerEmail = Preferences.Get("UserEmail", "null");
            var result = await _supabaseClient
                .From<Order>()
                .Where(x => x.SellerEmail == sellerEmail)
                .Get();

            Orders.Clear();
            foreach (var order in result.Models)
            {
                Orders.Add(order);
            }
            OrdersCOllectionView.ItemsSource = Orders;
        }
    
     private async void OnOrderActionClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is string action)
            {
                var order = (sender as VisualElement)?.Parent?.BindingContext as Order;

                if (order == null) return;

                switch (action)
                {
                    case "Accept":
                        AcceptOrder(order);
                        break;
                    case "Reject":
                        RejectOrder(order);
                        break;
                    case "Delivered":
                        DeliveredOrder(order);
                        break;

                    case "Chat":
                        OnChatClicked(order);
                        break;

                   
                }

            }

        } 
    private async void AcceptOrder(Order order)
        {
            var note = new Notification
            {
                title = "Order Rejected",
                content = $"Your order for {order.ProductName} has been accepted and is under processing.",
                user_email = order.buyer_email,
                id = Guid.NewGuid().ToString()
            };
            await _supabaseClient.From<Order>().Where(c => c.reference == order.reference).Set(v => v.status , "processing").Update();
            await _supabaseClient.From<Notification>().Insert(note);
        }
        private async void RejectOrder(Order order)
        {
            var note = new Notification
            {
                title = "Order Accepted",
                content = $"Your order for {order.ProductName} has been rejected chat with Seller for more info...",
                user_email = order.buyer_email,
                id = Guid.NewGuid().ToString()
            };
            await _supabaseClient.From<Order>().Where(c => c.reference == order.reference).Set(v => v.status, "rejected").Update();
            await _supabaseClient.From<Notification>().Insert(note);
        }
        private async void DeliveredOrder(Order order)
        {
            var note = new Notification
            {
                title = "Order Delivered",
                content = $"Your order for {order.ProductName} has been delivered. Click Confirm. Chat with Seller if you have not received your order.",
                user_email = order.buyer_email,
                id = Guid.NewGuid().ToString()
            };
            await _supabaseClient.From<Order>().Where(c => c.reference == order.reference).Set(v => v.status, "sent").Update();
            await _supabaseClient.From<Notification>().Insert(note);
        }
        private async void OnChatClicked(Order order)
        {
            var respo = await _supabaseClient.From<Chat>().Where(x => x.BuyerEmail == order.buyer_email && x.sellerEmail == order.SellerEmail).Get();
            var chat = respo?.Model;
            await Navigation.PushAsync(new ChatPage(chat.id, chat.sellerEmail, chat.BuyerEmail));
        }
    }
    
    }

