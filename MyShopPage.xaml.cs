using Supabase.Postgrest.Models;
using Supabase;
using System.Collections.ObjectModel;
using Supabase.Realtime;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;
using Newtonsoft.Json;
using Microsoft.Maui.ApplicationModel.Communication;
using MarketPlace.Services;
 
namespace MarketPlace;

public partial class MyShopPage : ContentPage
{
    private bool _isRunning;
    private Supabase.Client _supabaseClient;

    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    public ObservableCollection<Order> Orders { get; set; } = new();
    public ObservableCollection<Message> DashBoardMessages { get; set; } = new();
    public ObservableCollection<Order> SellerOrders { get; set; } = new();
    public ObservableCollection<Order> PendingOrders { get; set; } = new();
    public ObservableCollection<Order> Sales { get; set; } = new();
    public string useremail;

    public MyShopPage(AuthService authService, SupabaseService supabaseService)
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        UpdateOrdersUI();
        useremail = Preferences.Get("Useremail", "null");
        _supabaseService = supabaseService;
        _authService = authService;

       
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isRunning = true;
        StartLoop();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isRunning = false; // Stop loop when page is not visible
    }
    private async void NavigateToMenuPage(object sender , EventArgs e)
    {

        await Navigation.PushAsync(new MyShopMenuPage());
        //await Shell.Current.GoToAsync(nameof(MyShopMenuPage));
    }

    private async void StartLoop()
    {
        while (_isRunning)
        {
            await Task.Delay(60000); // Wait for 60 seconds to prevent UI freezing
            UpdateOrdersUI();
           FetchSellerOrders();
            updateSellerOrdersUI();
        }
    }
    private async void ListenToOrdersTable()
    {
        await _supabaseClient.From<Order>().On(ListenType.All, (sender, change) =>
        {
            var response = change.Payload.Data;
            Order orderChange = JsonConvert.DeserializeObject<Order>(response.ToString());
            
           // Debug.WriteLine(change.Payload.Data);
        });
    }
    private async void UpdateOrdersUI( )
    {
        int CompletedOrders = await GetAdditionalCompletedOrdersCount();
        int PendingOrders = await GetAdditionalPendingOrdersCount();
        int Balance = await GetWalletBalance();
        TotalSalesLabel.Text = CompletedOrders.ToString();
        RevenueLabel.Text = Balance.ToString();
        PendingOrdersLabel.Text = PendingOrders.ToString();

    }
    public async Task<int> GetAdditionalCompletedOrdersCount()
    {
        int AddedNumber = 0;
        string selleremail = Preferences.Get("UserEmail", "example@gmail.com").Trim();
        var result = await _supabaseClient
       .From<Order>()
       .Select(x => new object[] { x.SellerEmail, x.status })
       .Get();
        var ordersList = result.Models;
        if (ordersList.Count>0)
        {
            foreach (var order in ordersList)
            {
                if (order.SellerEmail.Trim() == selleremail & order.status.Trim() == "received")
                {
                    AddedNumber = AddedNumber + 1;
                    return AddedNumber;
                }
                else
                {
                    return 0;
                }
            }
        }
        else
        {
            Console.WriteLine("No Oders returens");
            return 0;
        }
        return 0;
        
    }
    public async Task<int> GetAdditionalPendingOrdersCount()
    {
        int AddedNumber = 0;
        string selleremail = Preferences.Get("UserEmail", "example@gmail.com").Trim();
        var result = await _supabaseClient
       .From<Order>()
       .Select(x => new object[] { x.SellerEmail, x.status })
       .Get();
        var ordersList = result.Models;
        if (ordersList.Count > 0)
        {
            foreach (var order in ordersList)
            {
                if (order.SellerEmail.Trim() == selleremail & order.status.Trim() == "pending")
                {
                    AddedNumber = AddedNumber + 1;
                     
                }
               
            }
        }
        else
        {
            Console.WriteLine("No Oders returens");
            return 0;
        }
        return AddedNumber;

    }
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SellerSettingsPage());
    }
    public void SetOrderStatusColor(Label statusLabel, string orderStatus)
    {
        orderStatus = orderStatus?.ToLower(); // Ensure it's lowercase for comparison

        statusLabel.TextColor = orderStatus switch
        {
            "completed" => Colors.Green,
            "pending" => Colors.Orange,
            "canceled" => Colors.Red,
            _ => Colors.Gray
        };
    }
    public async void updateSellerOrdersUI()
    {
        
    }
    private async void FetchSellerOrders()
    {
        
        string useremail = Preferences.Get("Useremail", "null");
        var result = await _supabaseClient
              .From<Order>()
              .Where(u => u.SellerEmail == useremail)
              .Get();
        if (result != null) {
            SellerOrders.Clear();
            Sales.Clear();
            PendingOrders.Clear();
            var orders = result.Models;
            foreach (var order in orders)
            {
                SellerOrders.Add(order);
                
                if(order.status.ToString() == "received")
                {
                    Sales.Add(order);
                }
                if (order.status.ToString() == "pending")
                {
                    PendingOrders.Add(order);
                }


            }
           
        }
        
        

    }
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
        //await Shell.Current.GoToAsync(nameof(HomePage1));
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
         await Navigation.PushAsync(new CartPage(  _authService,_supabaseService));
        //await Shell.Current.GoToAsync(nameof(CartPage));

    }
    private async void OnWalletClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyWalletPage());
    }
    private async void OnChatClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChatListPage(_authService,_supabaseService));
    }
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Settings());
        //await Shell.Current.GoToAsync(nameof(Settings));
    }

    private async void OnOrdersClicked(object sender, EventArgs e)
    {
         await Navigation.PushAsync(new SellerOrders());
        //await Shell.Current.GoToAsync(nameof(OrdersPage));

    }
    public async Task<bool> IsUserSeller(string Useremail)
    {
        var response = await _supabaseClient
            .From<Seller>()
            .Select("*")
            .Where(s => s.Email == Useremail)
            .Single();
        if (response == null)
        {
            return false;
        }
        // 
        else
        {
            return true;
            //await Navigation.PushAsync(new MyShopPage());

        }

    }
    private async void OnMyShopClicked(object sender, EventArgs e)
    {
        var answer = await IsUserSeller(useremail);
        if (answer == false)
        {
            var result = await DisplayAlert("Not Registered", "You are not a Seller yet. Do you want to register?", "Yes", "No");
            if (result)
            {
                await Navigation.PushAsync(new SellerRegistrationPage());
                //await Shell.Current.GoToAsync(nameof(SellerRegistrationPage));
            }
            else
            {
                return;
            }
        }
        else
        {
            await Navigation.PushAsync(new MyShopPage(_authService,_supabaseService));
            //await Shell.Current.GoToAsync(nameof(MyShopPage));
        }

    }




    public async Task<int> GetWalletBalance()
    {
        int balance = 0;
        string selleremail = Preferences.Get("UserEmail", "example@gmail.com").Trim();
        var result = await _supabaseClient
       .From<Wallet>()
       .Select(x => new object[] { x.user_email, x.balance })
       .Get();
        var walletsList = result.Models;
        if (walletsList.Count > 0)
        {
            foreach (var wallet in walletsList)
            {
                if (wallet.user_email == selleremail)
                {
                    balance = wallet.balance;
                    return balance;
                }
                else
                {
                    return 0;
                }
            }
        }
        else
        {
            Console.WriteLine("No Oders returens");
            return 0;
        }
        return 0;

    }
    private async void OnAddProductClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new AddProductPage());
        //await Shell.Current.GoToAsync(nameof(AddProductPage));
        await Navigation.PushAsync(new AddProductPage());

    }
    private async void OnEditProductClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new EditProductPage());
        //await Shell.Current.GoToAsync(nameof(EditProductPage));
        //await Navigation.PushAsync(new EditProductPage());
        await DisplayAlert("Hello", "This feature is under repair and is not available right now", "Ok");

    }
    private async void OnReportIssueClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReportIssue());
    }

    private async void OnFAQHelpClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new FAQHelpPagexaml());
        //await Shell.Current.GoToAsync(nameof(FAQHelpPagexaml));
        await Navigation.PushAsync(new FAQHelpPagexaml());
        
    }
   
}
