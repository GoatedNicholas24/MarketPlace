using Supabase.Gotrue.Mfa;
using Supabase.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
 
namespace MarketPlace;

public partial class CartCheckoutPage : ContentPage
{
    private List<Product> CartProducts;
     
    private decimal TotalDeliveryFee = 0;
    private decimal TotalProductsPrice = 0;
    public Supabase.Client _supabaseClient;
    public double Latitude = 0.00;
    public double Longitude = 0.00;
    public decimal TotalPayableAmount;
    public ObservableCollection<Order> Orders { get; set; } = new();
    public CartCheckoutPage(List<Product> cartProducts)
    {
        InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        CartProducts = cartProducts;
        CalculatePrices();
    }

    private void CalculatePrices()
    {
        TotalProductsPrice = CartProducts.Sum(p => p.Price*p.Quantity);
        ProductsTotalLabel.Text = TotalProductsPrice.ToString();
        ProductsTotalLabel_COD.Text = TotalProductsPrice.ToString();




    }
    private async void LocationCaptureClicked(object sender, EventArgs e)
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {

                
                foreach(var product in CartProducts)
                {
                    var sellerResponse = await _supabaseClient
                    .From<Seller>()
                    .Where(s => s.Email == product.Email)
                    .Get();
                    var seller = sellerResponse.Model;
                    var sellerLocation = seller.Location;
                    var finalSellerLocation = GetAreaKey(sellerLocation);
                 var buyerLocation = ReceiverLocationLabel.SelectedItem.ToString();
                    var buyerArea = ParseArea(buyerLocation);
                    var sellerArea = ParseArea(finalSellerLocation);
                    int deliveryFee = GetDeliveryFee(buyerArea, sellerArea);
                    product.DeliveryFee = deliveryFee;
                    TotalDeliveryFee += deliveryFee;
                   
                }
                DeliveryTotalLabel.Text = $"Delivery Fee: {TotalDeliveryFee:0.00}";
                DeliveryTotalLabel_COD.Text = $"Delivery Fee: {TotalDeliveryFee:0.00}";
                 TotalPayableAmount = TotalDeliveryFee + TotalProductsPrice;
                TotalPayableLabel.Text = $"UGX {TotalPayableAmount}";
                TotalPayableLabel_COD.Text = $"UGX {TotalPayableAmount}";
            }
            else
            {
                await DisplayAlert("Error", "Could not get location", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Location Error", ex.Message, "OK");
        }
    }





    string GetAreaKey(string fullLocation)
    {
        if (string.IsNullOrEmpty(fullLocation))
            return "unknown";

        var parts = fullLocation.Split(':');
        return parts[0].Trim(); // Get the part before the colon
    }

    private async void MobileMoneyPay_Clicked(object sender, EventArgs e)
    {
        MobileMoneyFrame.IsVisible = true;
        CashOnDeliveryFrame.IsVisible = false;   
    }

    private async void CashOnDelivery_Clicked(object sender, EventArgs e)
    {
        MobileMoneyFrame.IsVisible = false;
        CashOnDeliveryFrame.IsVisible = true;
    }

    private async void OnPlaceOrderCODClicked(object sender, EventArgs e)
    {
        string name = ReceiverNameEntry.Text?.Trim();
        string phone = ReceiverPhoneEntry.Text?.Trim();
        string location = ReceiverLocationLabel.SelectedItem?.ToString();
        string paymentPhone = PaymentPhoneEntry.Text;

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Missing Information", "Please enter receiver's full name.", "OK");

        }

        if (string.IsNullOrEmpty(phone))
        {
            await DisplayAlert("Missing Information", "Please enter receiver's phone number.", "OK");

        }
        if (string.IsNullOrEmpty(DetailedLocation.Text))
        {
            await DisplayAlert("Missing Information", "Please enter detailed location phone number.", "OK");

        }
        if (string.IsNullOrEmpty(paymentPhone))
        {
            await DisplayAlert("Missing Information", "Please enter payment phone number.", "OK");

        }

        // Validate Phone Number Format (Ugandan Example: 070, 077, 078, etc)
        if (!IsValidPhoneNumber(paymentPhone))
        {
            await DisplayAlert("Invalid Phone", "Please enter a valid Ugandan phone number in the format 256701234567.", "OK");

        }

        if (string.IsNullOrEmpty(location))
        {
            await DisplayAlert("Missing Information", "Please enter delivery location.", "OK");

        }
        var PaymentReference = Guid.NewGuid().ToString();
        await InitiatePayment(TotalDeliveryFee.ToString(), paymentPhone, PaymentReference, name, Preferences.Get("UserEmail", "null"), "nicholaswatiti23@gmail.com");

    }
    public enum LocationArea
    {
        Bwegiragye,
        IshakaTown,
        KIUIshaka,
        Abuja,
        Bushenyi,
        Kabwohe,
        Lagos,
        Mbarara,
        Unknown
    }

    private LocationArea ParseArea(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return LocationArea.Unknown;

        var key = location.Trim().ToLower();

        return key switch
        {
            "bwegiragye" => LocationArea.Bwegiragye,
            "ishaka town" => LocationArea.IshakaTown,
            "kiu ishaka" => LocationArea.KIUIshaka,
            "abuja" => LocationArea.Abuja,
            "bushenyi" => LocationArea.Bushenyi,
            "kabwohe" => LocationArea.Kabwohe,
            "lagos" => LocationArea.Lagos,
            _ => LocationArea.Unknown
        };
    }
    private int GetDeliveryFee(LocationArea from, LocationArea to)
    {
        // Normalize: make the tuple order-insensitive
        var route = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.Bwegiragye, LocationArea.IshakaTown),
        (LocationArea.Bwegiragye, LocationArea.KIUIshaka),
        (LocationArea.Abuja, LocationArea.KIUIshaka),
        (LocationArea.Bushenyi, LocationArea.Kabwohe),
        (LocationArea.Lagos, LocationArea.IshakaTown),
        (LocationArea.Lagos, LocationArea.KIUIshaka),
        (LocationArea.Lagos, LocationArea.Lagos), // Lagos local
    };

        var tier3k = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.KIUIshaka, LocationArea.Bushenyi),
        (LocationArea.IshakaTown, LocationArea.Bushenyi),
        (LocationArea.Lagos, LocationArea.Bushenyi),
    };

        var tier4k = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.Kabwohe, LocationArea.Lagos),
        (LocationArea.Abuja, LocationArea.Bushenyi),
    };

        var tier6k = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.Bwegiragye, LocationArea.Bushenyi),
        (LocationArea.Bwegiragye, LocationArea.Kabwohe),
    };

        var tier8k = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.Bushenyi, LocationArea.Mbarara), // if Mbarara is added later
    };

        var pair = (from, to);
        var reversed = (to, from);

        if (route.Contains(pair) || route.Contains(reversed))
            return 2000;
        if (tier3k.Contains(pair) || tier3k.Contains(reversed))
            return 3000;
        if (tier4k.Contains(pair) || tier4k.Contains(reversed))
            return 4000;
        if (tier6k.Contains(pair) || tier6k.Contains(reversed))
            return 6000;
        if (tier8k.Contains(pair) || tier8k.Contains(reversed))
            return 8000;

        return 2000; // Default fallback
    }
    private async void OnPayNowClicked(object sender, EventArgs e)
    {
        string name = ReceiverNameEntry.Text?.Trim();
        string phone = ReceiverPhoneEntry.Text?.Trim();
        string location = ReceiverLocationLabel.SelectedItem?.ToString();
        string paymentPhone = PaymentPhoneEntry.Text;

        if (string.IsNullOrEmpty(name))
        {
           await  DisplayAlert("Missing Information", "Please enter receiver's full name.", "OK");
            
        }

        if (string.IsNullOrEmpty(phone))
        {
           await DisplayAlert("Missing Information", "Please enter receiver's phone number.", "OK");
            
        }
        if (string.IsNullOrEmpty(DetailedLocation.Text))
        {
            await DisplayAlert("Missing Information", "Please enter detailed location phone number.", "OK");

        }
        if (string.IsNullOrEmpty(paymentPhone))
        {
            await DisplayAlert("Missing Information", "Please enter payment phone number.", "OK");

        }

        // Validate Phone Number Format (Ugandan Example: 070, 077, 078, etc)
        if (!IsValidPhoneNumber(paymentPhone))
        {
            await DisplayAlert("Invalid Phone", "Please enter a valid Ugandan phone number in the format 256701234567.", "OK");
           
        }

        if (string.IsNullOrEmpty(location))
        {
           await  DisplayAlert("Missing Information", "Please enter delivery location.", "OK");
             
        }
        var PaymentReference = Guid.NewGuid().ToString();
        await InitiatePayment(TotalPayableAmount.ToString(), paymentPhone, PaymentReference, name, Preferences.Get("UserEmail", "null"), "nicholaswatiti23@gmail.com");
       
    }
    private async Task InitiatePayment(string amount, string phone, string reference, string buyer_name, string buyer_email, string SellerEmail)
    {
        var paymentRequest = new
        {
            username = "64a52df432f2b748",
            password = "49b3629b4feea447",
            action = "mmdeposit",
            amount = amount,
            currency = "UGX",
            phone = phone,
            reference = reference,
            reason = $"{buyer_name + buyer_email + " " + "Deposit to" + SellerEmail}"

        };

        // Make a POST request to initiate payment
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(5); // Increase timeout to 5 minutes
            var response = await client.PostAsJsonAsync("https://www.easypay.co.ug/api/", paymentRequest);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine(jsonResponse);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var paymentResponse = JsonSerializer.Deserialize<EasyPayResponse>(jsonResponse, options);

                if (paymentResponse != null && paymentResponse.success == 1)
                {
                    await DisplayAlert("Payment Success", $"Amount: {paymentResponse.details.amount}\nReference: {paymentResponse.details.reference}", "OK");
                    await CheckPaymentStatus(reference);
                }
                else
                {
                    await DisplayAlert("Payment Failed", "Could not initiate payment. Please try again.", "OK");
                }

            }
        }
    }
    public class EasyPayResponse
    {
        public Details details { get; set; }
        public int success { get; set; }
        public string data { get; set; }
    }

    public class Details
    {
        public string phone { get; set; }
        public string reference { get; set; }
        public string telecomId { get; set; }
        public string transactionId { get; set; }
        public string amount { get; set; }
        public string reason { get; set; }
        public string currencyCode { get; set; }
    }
    private async Task CheckPaymentStatus(string reference)
    {
        while (true)
        {
            await Task.Delay(10000); // Check every 5 seconds

            var payload = new
            {
                username = "64a52df432f2b748",
                password = "49b3629b4feea447",
                action = "mmstatus",
                reference = reference
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync("https://www.easypay.co.ug/api/", payload);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(jsonResponse);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var easyPayResponse = JsonSerializer.Deserialize<EasyPayResponse>(jsonResponse, options);



                    if (easyPayResponse.success == 1 && easyPayResponse.data.Contains("Success"))
                    {
                        InsertOrders();
                        UpdatePopularityScores();
                        

                        break;
                    }
                    else
                    {

                        break;
                    }
                }

            }
        }
    }
    
    private async void InsertOrders()
    {
        
            foreach (var product in CartProducts)
            {
             var OrderReference = Guid.NewGuid().ToString();
            var amount = product.Quantity * product.Price + product.DeliveryFee;
                var order = new Order
                {
                    reference = OrderReference,
                    buyer_name = ReceiverNameEntry.Text,
                    amount = amount.ToString(),
                    PaymentMethod = "Mobile Money",
                    buyer_email = Preferences.Get("UserEmail", "null"),
                    status = "processing",
                    buyer_phone = ReceiverPhoneEntry.Text,
                    Quantity = product.Quantity.ToString(),
                    SellerEmail = product.Email,
                    ProductName = product.Name,
                    delivery_address = ReceiverLocationLabel.SelectedItem.ToString() + $" :{DetailedLocation.Text}",
                    DeliveryFee = product.DeliveryFee
                };
            await _supabaseClient.From<Order>().Insert(order);
            await _supabaseClient
           .From<Notification>()
            .Insert(new Notification
          {
        user_email = product.Email,
        title = "New Order",
        content = $"You have a new order from {order.buyer_name}",
        order_id = order.reference,
        type = "order_placed"
           });

        }
         
    }
    private async void UpdatePopularityScores()
    {
        foreach (var _selectedProduct in CartProducts)
        {
            
            var result = await _supabaseClient.From<Product>().Where(p => p.ProductId == _selectedProduct.ProductId).Get();
            int score = result.Model.PopularityScore;
            int newscore = score + 20;
            if (result == null)
            {
                return;
            }
            var response = await _supabaseClient.From<Product>().Where(p => p.ProductId == _selectedProduct.ProductId).Set(x => x.PopularityScore, newscore).Update();
        }
    }
    static bool IsValidPhoneNumber(string phoneNumber)
    {
        string pattern = @"^256\d{9}$";  // Ensures it starts with 256 and is followed by exactly 9 digits (total 12 digits)
        Regex regex = new Regex(pattern);
        return regex.IsMatch(phoneNumber);
    }
    
}