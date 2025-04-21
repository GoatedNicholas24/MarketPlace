using System.Net.Http.Json;
using System.Text;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using Supabase.Interfaces;
using System.Reflection;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
namespace MarketPlace;
 
public partial class CheckoutPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public string _productId;
    public Product _product;
    public Supabase.Client _supabaseClient;
    public CheckoutPage(Product product)
    {
        InitializeComponent();
        
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };

        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
        _product = product;
        ProductNameLabel.Text = _product.Name;
        ProductPriceLabel.Text = $" UGX {_product.Price}";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");




    }
    public async Task<string> testSupabaseFunction(decimal amount, string phoneNumber, string refId)
    {
        var requestData = new
        {
            amount = amount,
            phoneNumber = phoneNumber,
            refId = refId
        };

        var json = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        string _supabasefunU = "https://bbgpafulnowlgduckgie.supabase.co/functions/v1/supabaseDeposit";

        var response = await _httpClient.PostAsync(_supabasefunU, content);

        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);
        await DisplayAlert("jeee", responseString, "ok");
        if (!response.IsSuccessStatusCode)
        {
            // You can handle errors more cleanly here
            //  throw new Exception($"Error: {response.StatusCode} | {responseString}");
            await DisplayAlert("jeee", responseString, "ok");
        }

        return responseString;
    }

    string GetAreaKey(string fullLocation)
    {
        if (string.IsNullOrEmpty(fullLocation))
            return "unknown";

        var parts = fullLocation.Split(':');
        return parts[0].Trim(); // Get the part before the colon
    }
    

    public double Latitude=0.00;
    public double Longitude=0.00;
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
        {
            DisplayAlert("Errooootr", "Empty", "Ok");
            return LocationArea.Unknown; }

        var key = location.Trim().ToLower();

        return key switch
        {
            "Bwegiragye" => LocationArea.Bwegiragye,
            "ishaka town" => LocationArea.IshakaTown,
            "kiu ishaka" => LocationArea.KIUIshaka,
            "mbarara" => LocationArea.Mbarara,
            "abuja" => LocationArea.Abuja,
            "bushenyi" => LocationArea.Bushenyi,
            "kabwohe" => LocationArea.Kabwohe,
            "lagos" => LocationArea.Lagos,
            _ => LocationArea.Unknown
        };
    }
    private int GetDeliveryFee(LocationArea from, LocationArea to)
    {

        var zero = new HashSet<(LocationArea, LocationArea)>
        {
            (LocationArea.Lagos, LocationArea.Lagos),
            (LocationArea.Bushenyi, LocationArea.Bushenyi)
        };
        {
            // Normalize: make the tuple order-insensitive
            var route = new HashSet<(LocationArea, LocationArea)>
    {
        (LocationArea.Bwegiragye, LocationArea.IshakaTown),
        (LocationArea.Bwegiragye, LocationArea.KIUIshaka),
        (LocationArea.Abuja, LocationArea.KIUIshaka),
        (LocationArea.Bushenyi, LocationArea.Kabwohe),
        (LocationArea.Lagos, LocationArea.IshakaTown),
        (LocationArea.Lagos, LocationArea.KIUIshaka)
        // Lagos local
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
            if (zero.Contains(pair) || zero.Contains(reversed))
                return 0;

            return 3000; // Default fallback
        }
    }


    private async void LocationCaptureClicked(object sender, EventArgs e)
    {
        try
        {

            if (ReceiverLocationLabel.SelectedItem == null)
            {
                await DisplayAlert("Missing Info", "Please select your location.", "OK");
                return;
            }

          
            int deliveryFee = 0;
            try {
                  
                 

                var userResp = await _supabaseClient.From<Seller>().Where(p => p.Email == _product.Email).Get();
                var userModel = userResp.Model;
                if (userModel == null) { return; }
                var sellerLocation = userModel.Location;

                if (sellerLocation == null)
                {
                    await DisplayAlert("Error", "Seller not found.", "OK");
                    return;
                }
                var finalSellerLocation = GetAreaKey(sellerLocation);
                var buyerLocation = ReceiverLocationLabel.SelectedItem.ToString();
                var buyerArea = ParseArea(buyerLocation);
                var sellerArea = ParseArea(finalSellerLocation);

                 deliveryFee = GetDeliveryFee(buyerArea, sellerArea);
                DeliveryTotalLabel.Text = deliveryFee.ToString();
                DeliveryTotalLabel_COD.Text = deliveryFee.ToString();
            }
            catch(Exception ex) {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
           
            


             

                
                if(ProductQuanitYLabel.Text == null)
                {
                    await DisplayAlert("Please Enter quantity", "Proceed", "Ok");
                    return;
                }
                if(!int.TryParse(ProductQuanitYLabel.Text , out var quantity))
                {
                    await DisplayAlert("Please Enter valid quantity", "In numbers(eg.1,2..)", "Ok");
                    return;
                }
                var productsTotal = _product.Price * quantity;
                ProductsTotalLabel.Text = productsTotal.ToString();
                var TotalCost = _product.Price * quantity + deliveryFee;
                PaymentConfirmationFrameLabel.Text = $"You will be debited with a delivery fee of {deliveryFee} and a sub-total product payment of {_product.Price * quantity} resulting into a total charge of {_product.Price * quantity + deliveryFee}. Click confirm if you accept";
                PaymentConfirmationFrameLabel2.Text = $"You will be debited with a delivery fee of {deliveryFee}. Confirm if you accept";

            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Location Error", ex.Message, "OK");
        }
    }
   
    
    private async void MobileMoneyButtonClicked(object sender, EventArgs e)
    {
        MobileMoneyFrame.IsVisible = true;
        CashOnDeliveryFrame.IsVisible = false;
    }
    private async void CashOnDeliveryButtonClicked(object sender, EventArgs e)
    {
        MobileMoneyFrame.IsVisible = false;
        CashOnDeliveryFrame.IsVisible = true;
    }
    private async void CancelClicked1(object sender, EventArgs e)
    {
        PaymentConfirmationFrame.IsVisible = false;
    }
    private async void ConfirmClicked1(object sender, EventArgs e)
    {
      //  await testSupabaseFunction(1, 46733123454.ToString(), Guid.NewGuid().ToString());
         startPaymentProcess();
        PaymentConfirmationFrame.IsVisible = false;
    }
    private async void CancelClicked2(object sender, EventArgs e)
    {
        PaymentConfirmationFrame2.IsVisible = false;
    }
    private async void ConfirmClicked2(object sender, EventArgs e)
    {
        startPaymentProcess2();
        PaymentConfirmationFrame2.IsVisible = false;
    }
    private async void startPaymentProcess()
    {
        try
        {
            // Get user input from Entry fields
            string receiverName = string.IsNullOrWhiteSpace(ReceiverNameEntry.Text) ? null : ReceiverNameEntry.Text.Trim();
            string location = string.IsNullOrWhiteSpace(ReceiverLocationLabel.SelectedItem.ToString()) ? null : ReceiverLocationLabel.SelectedItem.ToString().Trim();
            string phoneNumber = string.IsNullOrWhiteSpace(ReceiverPhoneEntry.Text) ? null : ReceiverPhoneEntry.Text.Trim();
            string quantityText = ProductQuanitYLabel.Text;
            string paymentPhone = string.IsNullOrWhiteSpace(PaymentPhoneEntry.Text) ? null : PaymentPhoneEntry.Text.Trim();

            // Validate input fields
            if (string.IsNullOrWhiteSpace(receiverName) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(quantityText) ||
                string.IsNullOrWhiteSpace(paymentPhone))
            {
                await DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            // Convert quantity to integer
            if (!int.TryParse(quantityText, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Error", "Enter a valid product quantity.", "OK");
                return;
            }
      
                var useremail = Preferences.Get("UserEmail", "null");
                var UserName = "";
             
            var Result = await _supabaseClient
                 .From<User>()
                 .Select("*")
                 .Get();
            if (Result == null)
            {

            }
            else
            {
                var usernames = Result.Models;
                foreach (var username in usernames)
                {
                    if (username.email == useremail)
                    {
                        UserName = username.username;
                    }
                }
                var deliveryFee = DeliveryTotalLabel.Text;
                if (
                    !int.TryParse(deliveryFee, out int deliveryfee))
                {
                    return;
                }
                var TotalAmount = _product.Price * quantity + deliveryfee;
                var OrderReference = Guid.NewGuid().ToString();
                var order = new Order
                {
                    reference = OrderReference,
                    buyer_name = UserName,
                    amount = TotalAmount.ToString(),
                    PaymentMethod = "Mobile Money",
                    buyer_email = useremail,
                    status = "processing",
                    buyer_phone = paymentPhone,
                    Quantity = quantity.ToString(),
                    SellerEmail = _product.Email,
                    ProductName = _product.Name,
                    delivery_address = location + $" : {DetailedLocation.Text}",
                    DeliveryFee = deliveryfee




                };
                await InitiatePayment(order);
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
    }
    private async void startPaymentProcess2()
    {
        try
        {
            // Get user input from Entry fields
            string receiverName = string.IsNullOrWhiteSpace(ReceiverNameEntry.Text) ? null : ReceiverNameEntry.Text.Trim();
            string location = string.IsNullOrWhiteSpace(ReceiverLocationLabel.SelectedItem.ToString()) ? null : ReceiverLocationLabel.SelectedItem.ToString().Trim();
            string phoneNumber = string.IsNullOrWhiteSpace(ReceiverPhoneEntry.Text) ? null : ReceiverPhoneEntry.Text.Trim();
            string quantityText = "0";
            string paymentPhone = string.IsNullOrWhiteSpace(PaymentPhoneEntry.Text) ? null : PaymentPhoneEntry.Text.Trim();

            // Validate input fields
            if (string.IsNullOrWhiteSpace(receiverName) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(quantityText) ||
                string.IsNullOrWhiteSpace(paymentPhone))
            {
                await DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }

            // Convert quantity to integer
            if (!int.TryParse(quantityText, out int quantity) || quantity < 0)
            {
                await DisplayAlert("Error", "Enter a valid product quantity.", "OK");
                return;
            }
            try {
                var useremail = Preferences.Get("UserEmail", "null");
                var UserName = "";
                var Result = await _supabaseClient
                     .From<User>()
                     .Select("*")
                     .Get();
                if (Result == null)
                {

                }
                else
                {
                    var usernames = Result.Models;
                    foreach (var username in usernames)
                    {
                        if (username.email == useremail)
                        {
                            UserName = username.username;
                        }
                    }
                    var deliveryFee = DeliveryTotalLabel_COD.Text;
                    if (
                        !int.TryParse(deliveryFee, out int deliveryfee))
                    {
                        return;
                    }
                    var TotalAmount = _product.Price * quantity + deliveryfee;
                    var OrderReference = Guid.NewGuid().ToString();
                    var order = new Order
                    {
                        reference = OrderReference,
                        buyer_name = UserName,
                        amount = TotalAmount.ToString(),
                        PaymentMethod = "Mobile Money",
                        buyer_email = useremail,
                        status = "processing",
                        buyer_phone = paymentPhone,
                        Quantity = quantity.ToString(),
                        SellerEmail = _product.Email,
                        ProductName = _product.Name,
                        delivery_address = location + $" : {DetailedLocation.Text}"




                    };
                    await InitiatePayment(order);
                }
            } catch(Exception ex) { await DisplayAlert ("Error 101", ex.Message, "Kale"); }         
           

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
    }
    private async void InitiateButtonClicked1(object sender, EventArgs e)
    {
        PaymentConfirmationFrame.IsVisible = true;
      
    }
    private async Task CheckPaymentStatus(Order order)
    {
        while (true)
        {
             
            await Task.Delay(20000); // Check every 5 seconds

            var payload = new
            {
                username = "64a52df432f2b748",
                password = "49b3629b4feea447",
                action = "mmstatus",
                reference = order.reference
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

                    var easyPayResponse = System.Text.Json.JsonSerializer.Deserialize<PaymentResponse>(jsonResponse, options);



                    if (easyPayResponse.success == 1 && easyPayResponse.data.status == "Success")
                    {
                        
                        await _supabaseClient.From<Order>().Insert(order);

                        await _supabaseClient
            .From<Notification>()
            .Insert(new Notification
            {
                id = Guid.NewGuid().ToString(),
                user_email = _product.Email,
                title = "New Order",
                content = $"You have a new order from {order.buyer_name} for {order.Quantity} of {order.ProductName} at {order.amount} with delivery fee of {order.DeliveryFee}",
                order_id = order.reference,
                type = "order_placed"
            });


                        UpdatePopularityScore();

                        break;
                    }
                    else
                    {
                        DisplayAlert("Debug", "The transaction was not successful.", "OK");
                        break;
                    }
                }

            }
        }
    }

    private async void UpdatePopularityScore()
    {
        Task.Delay(3000);
        var result = await _supabaseClient.From<Product>().Where(p => p.ProductId == _product.ProductId).Get();
        int score = result.Model.PopularityScore;
        int newscore = score + 20;
        if (result == null)
        {
            return;
        }
        var response = await _supabaseClient.From<Product>().Where(p => p.ProductId == _product.ProductId).Set(x => x.PopularityScore, newscore).Update();
    }

    private async Task InitiatePayment(Order order)
    {
        var paymentRequest = new
        {
            username = "64a52df432f2b748",
            password = "49b3629b4feea447",
            action = "mmdeposit",
            amount = order.amount,
            currency = "UGX",
            phone = order.buyer_phone,
            reference = order.reference,
            reason = $"{order.buyer_name + order.buyer_email + " " + "Deposit to" + order.SellerEmail}"

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

                var paymentResponse = System.Text
                    .Json.JsonSerializer.Deserialize<EasyPayResponse>(jsonResponse, options);

                if (paymentResponse != null && paymentResponse.success == 1)
                {
                    await DisplayAlert("Payment Success", $"Amount: {paymentResponse.details.amount}\nReference: {paymentResponse.details.reference}", "OK");
                    await CheckPaymentStatus(order);
                }
                else
                {
                    await DisplayAlert("Payment Failed", "Could not initiate payment. Please try again.", "OK");
                }

            }
        }
        }
    



    private async void InitiateButtonClicked2(object sender, EventArgs e)
    {
        
        
        try
        {
            // Get user input from Entry fields
            string receiverName = string.IsNullOrWhiteSpace(ReceiverNameEntry.Text) ? null : ReceiverNameEntry.Text.Trim();
            string location = string.IsNullOrWhiteSpace(ReceiverLocationLabel.SelectedItem.ToString()) ? null : ReceiverLocationLabel.SelectedItem.ToString().Trim();
            string phoneNumber = string.IsNullOrWhiteSpace( ReceiverPhoneEntry.Text) ? null : ReceiverPhoneEntry.Text.Trim();
            string quantityText = ProductQuanitYLabel.Text;
            string paymentPhone = string.IsNullOrWhiteSpace(PaymentPhoneEntry.Text) ? null : PaymentPhoneEntry.Text.Trim();

            // Validate input fields
            if (string.IsNullOrWhiteSpace(receiverName) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(quantityText) ||
                string.IsNullOrWhiteSpace(paymentPhone))
            {
                await DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
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


            // Convert quantity to integer
            if (!int.TryParse(quantityText, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Error", "Enter a valid product quantity.", "OK");
                return;
            }
            var useremail = Preferences.Get("UserEmail", "null");
            var UserName = "";
            var Result = await _supabaseClient
                 .From<User>()
                 .Select("*")
                 .Get();
            if (Result == null)
            {

            }
            else
            {
                var usernames = Result.Models;
                foreach (var username in usernames)
                {
                    if (username.email == useremail)
                    {
                        UserName = username.username;
                    }
                }
                var deliveryFee = DeliveryTotalLabel_COD.Text;
                if (
                    !int.TryParse(deliveryFee, out int deliveryfee))
                {
                    return;
                }
                 
                var OrderReference = Guid.NewGuid().ToString();
                var order = new Order
                {
                    reference = OrderReference,
                    buyer_name = UserName,
                    amount = deliveryfee.ToString(),
                    PaymentMethod = "Cash On Delivery",
                    buyer_email = useremail,
                    status = "processing",
                    buyer_phone = paymentPhone,
                    Quantity = quantity.ToString(),
                    SellerEmail = _product.Email,
                    ProductName = _product.Name,
                    delivery_address = location




                };
                await InitiatePayment(order);
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
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
    public class PaymentResponse
    {
        public int success { get; set; }
        public PaymentData data { get; set; }
    }
    public class PaymentData
    {
        public string status { get; set; }
        public string amount { get; set; }
        public string charge { get; set; }
        public string reference { get; set; }
    }

    static bool IsValidPhoneNumber(string phoneNumber)
    {
        string pattern = @"^256\d{9}$";  // Ensures it starts with 256 and is followed by exactly 9 digits (total 12 digits)
        Regex regex = new Regex(pattern);
        return regex.IsMatch(phoneNumber);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}

