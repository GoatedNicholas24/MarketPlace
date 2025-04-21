using Supabase;
using Supabase.Interfaces;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using Supabase.Postgrest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketPlace.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _supabase;

        public SupabaseService(Supabase.Client supabase)
        {
            _supabase = supabase;
        } 
        public IPostgrestTable<T> From<T>() where T : BaseModel, new()
        {
            return _supabase.From<T>();
        }

        public async Task<List<Notification>> GetUnreadNotifications(string userEmail)
        {
            var response = await _supabase
                .From<Notification>()
                .Where(x => x.user_email == userEmail && x.is_read != "true")
                .Get();

            return response.Models;
        }

        public async Task<bool> MarkNotificationAsRead(string notificationId)
        {
            try
            {
                var response = await _supabase
                    .From<Notification>()
                    .Where(x => x.id == notificationId)
                    .Set(x => x.is_read, "true")
                    .Update();

                return response != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Product>> GetFeaturedProducts()
        {
            var response = await _supabase
                .From<Product>()
                .Where(x => x.IsFeatured == true)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(5)
                .Get();

            return response.Models;
        }

        public async Task<List<Product>> GetProducts(string category, int page, int pageSize)
        {
            var query = _supabase
                .From<Product>()
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Range((page - 1) * pageSize, page * pageSize - 1);

            if (category != "all")
            {
                query = query.Where(x => x.Category == category);
            }

            var response = await query.Get();
            return response.Models;
        }

        public async Task<List<Ad>> GetActiveAds()
        {
            var response = await _supabase
                .From<Ad>()
                .Where(x => x.IsActive == true)
                .Where(x => x.StartDate <= DateTime.UtcNow)
                .Where(x => x.EndDate >= DateTime.UtcNow)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models;
        }

        public async Task AddToWishlist(string userId, string productId)
        {
            var wishlistItem = new WishListItem
            {
                Id = Guid.NewGuid().ToString(),
                User_Email = userId,
                Product_Id = productId
            };

            await _supabase.From<WishListItem>().Insert(wishlistItem);
        }

        public async Task AddToCart(string userId, string productId, int quantity)
        {
            var cartItem = new Cart
            {
                Id = Guid.NewGuid().ToString(),
                User_Email = userId,
                Product_Id = productId,
                Quantity = quantity.ToString()
            };

            await _supabase.From<Cart>().Insert(cartItem);
        }

        public async Task<bool> RemoveFromCart(string productId)
        {
            try
            {
                var response =   _supabase
                    .From<Cart>()
                    .Where(x => x.Product_Id == productId)
                    .Delete();

                return response != null;
            }
            catch
            {
                return false;
            }
        }
        public async Task<Supabase.Client> GetClient()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            Supabase.Client supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
            return supabaseClient;
        }

        public async Task<Product> GetProductById(string productId)
        {
            var response = await _supabase
                .From<Product>()
                .Where(x => x.ProductId == productId)
                .Single();

            return response;
        }

        public async Task<List<Cart>> GetCartItems(string userEmail)
        {
            var response = await _supabase
                .From<Cart>()
                .Where(x => x.User_Email == userEmail)
                .Get();

            return response.Models;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var response = await _supabase
                .From<User>()
                .Where(u => u.email == email)
                .Single();
            
            return response;
        }

        public async Task<List<Product>> GetWishlist(string userId)
        {
            try
            {
                // First get wishlist entries for user
                var wishlistEntries = await _supabase
                    .From<WishListItem>()
                    .Where(w => w.User_Email == userId)
                    .Get();

                if (wishlistEntries.Models.Count == 0)
                    return new List<Product>();

                // Get product IDs from wishlist
                var productIds = wishlistEntries.Models.Select(w => w.Product_Id).ToList();

                var productsResultModels = new List<Product>();
                // Load the actual products
                foreach (var productId in productIds)
                {
                    var productsResult = await _supabase
                        .From<Product>()
                        .Where(p => p.ProductId == productId)
                        .Get();

                    if (productsResult?.Model != null)
                    {
                        // Handle null or empty images
                        if (string.IsNullOrEmpty(productsResult.Model.Images))
                        {
                            // Use a proper placeholder image URL
                            productsResult.Model.Images = "https://via.placeholder.com/300x300?text=No+Image";
                        }
                        else if (!productsResult.Model.Images.StartsWith("http"))
                        {
                            // If the image is not a full URL, prepend the base URL
                            // Remove any leading slashes from the image path
                            var cleanImagePath = productsResult.Model.Images.TrimStart('/');
                            productsResult.Model.Images = $"https://bbgpafulnowlgduckgie.supabase.co/storage/v1/object/public/products/{cleanImagePath}";
                        }

                        // Debug: Print the final image URL
                        System.Diagnostics.Debug.WriteLine($"Product {productId} image URL: {productsResult.Model.Images}");

                        productsResultModels.Add(productsResult.Model);
                    }
                }

                return productsResultModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetWishlist: {ex.Message}");
                throw new Exception($"Failed to get wishlist: {ex.Message}");
            }
        }

        public async Task RemoveFromWishlist(string userId, string productId)
        {
            try
            {
                var response =   _supabase
                    .From<WishListItem>()
                    .Where(w => w.User_Email == userId && w.Product_Id == productId)
                    .Delete();

                if (response == null)
                {
                    throw new Exception("Failed to remove item from wishlist");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove from wishlist: {ex.Message}");
            }
        }

        public async Task<List<Order>> GetOrders(string userId)
        {
            try
            {
                var response = await _supabase
                    .From<Order>()
                    .Where(x => x.buyer_email == userId)
                    .Order(x => x.created_at, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get orders: {ex.Message}");
            }
        }

        public async Task CancelOrder(string orderReference)
        {
            try
            {
                // Update order status
                var orderResponse = await _supabase
                    .From<Order>()
                    .Where(x => x.reference == orderReference)
                    .Set(x => x.status, "cancelled_by_user")
                    .Update();

                if (orderResponse == null)
                {
                    throw new Exception("Failed to update order status");
                }

                // Get order details for notification
                var order = orderResponse.Model;
                if (order != null)
                {
                    // Create notification for seller
                    var notification = new Notification
                    {
                        title = "Order Canceled",
                        content = $"The order for {order.ProductName} from {order.buyer_name} has been canceled. Chat with buyer for more info...",
                        user_email = order.SellerEmail,
                        id = Guid.NewGuid().ToString()
                    };

                    await _supabase.From<Notification>().Insert(notification);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel order: {ex.Message}");
            }
        }

        public async Task ConfirmOrder(string orderReference)
        {
            try
            {
                // Update order status
                var orderResponse = await _supabase
                    .From<Order>()
                    .Where(x => x.reference == orderReference)
                    .Set(x => x.status, "received")
                    .Update();

                if (orderResponse == null)
                {
                    throw new Exception("Failed to update order status");
                }

                // Get order details
                var order = orderResponse.Model;
                if (order != null)
                {
                    // Update seller's wallet
                    var walletResponse = await _supabase
                        .From<Wallet>()
                        .Where(x => x.user_email == order.SellerEmail)
                        .Get();

                    if (walletResponse != null && walletResponse.Model != null)
                    {
                        int currentBalance = walletResponse.Model.balance;
                        await _supabase
                            .From<Wallet>()
                            .Where(x => x.user_email == order.SellerEmail)
                            .Set(x => x.balance, currentBalance + order.amount)
                            .Update();
                    }

                    // Create notification for seller
                    var notification = new Notification
                    {
                        title = "Receipt Confirmed",
                        content = $"{order.ProductName} delivered to {order.buyer_name} has been received. Check your wallet to confirm...",
                        user_email = order.SellerEmail,
                        id = Guid.NewGuid().ToString()
                    };

                    await _supabase.From<Notification>().Insert(notification);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to confirm order: {ex.Message}");
            }
        }

        public async Task<Chat> GetOrCreateChat(string buyerEmail, string sellerEmail)
        {
            try
            {
                // Check if chat exists
                var existingChat = await _supabase
                    .From<Chat>()
                    .Where(x => x.BuyerEmail == buyerEmail && x.sellerEmail == sellerEmail)
                    .Get();

                if (existingChat != null && existingChat.Model != null)
                {
                    return existingChat.Model;
                }

                // Create new chat if it doesn't exist
                var newChat = new Chat
                {
                    id = Guid.NewGuid().ToString(),
                    BuyerEmail = buyerEmail,
                    sellerEmail = sellerEmail
                };

                var response = await _supabase.From<Chat>().Insert(newChat);
                return response.Model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get or create chat: {ex.Message}");
            }
        }
    }
} 