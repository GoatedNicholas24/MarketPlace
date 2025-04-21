using System.Collections.ObjectModel;
using System.Collections;
using System.Timers;
using Supabase;
using System.Windows.Input;
using MarketPlace.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MarketPlace;

public partial class HomePage1 : ContentPage
{
    private bool _isRunning;
    private int currentIndex = 0;
    private System.Timers.Timer scrollTimer;
    private string _currentCategory = "all";
    private int _currentPage = 1;
    private const int PageSize = 10;
    private bool _isLoading = false;

    public ObservableCollection<Ad> Adverts { get; set; } = new();
    public ObservableCollection<Product> Products { get; set; } = new();
    public ObservableCollection<Product> FilteredProducts { get; set; } = new();

    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    
    private ObservableCollection<Product> FeaturedProducts;

    public ICommand RefreshCommand { get; }
    public ICommand LoadMoreCommand { get; }

    private User _currentUser;
    private ImageButton _avatarButton;

    public HomePage1(SupabaseService supabaseService, AuthService authService)
    {
        InitializeComponent();
        _supabaseService = supabaseService;
        _authService = authService;
        FeaturedProducts = new ObservableCollection<Product>();
        BindingContext = this;

        Products = new ObservableCollection<Product>();
        Adverts = new ObservableCollection<Ad>();

        RefreshCommand = new AsyncRelayCommand(RefreshData);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreProducts);

        LoadInitialData();
        LoadUserDetails();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(  _authService,_supabaseService));
    }

    private async void ChatIcon_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChatListPage(_authService,_supabaseService));
    }

    private async void HomeMenuButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomeMenuPage(_authService,_supabaseService));
    }

    private async void OnAddToWishlist(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var product = button?.BindingContext as Product;
        if (product != null)
        {
            await _supabaseService.AddToWishlist(Preferences.Get("UserEmail",""),product.ProductId);
        }
    }

    private async void OnAddToCart(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var product = button?.BindingContext as Product;
        string result = await DisplayPromptAsync(
   "Enter Quantity",
   "In what quantity should this item be added to your cart?",
   placeholder: "e.g. 1",
   maxLength: 3,
   keyboard: Microsoft.Maui.Keyboard.Numeric
);
        if (product != null)
        {
            if (int.TryParse(result, out int qua))
            {
                await _supabaseService.AddToCart(Preferences.Get("UserEmail", ""), product.ProductId, qua);
            }
            else { return; }
        }
    }

    private async void OnProductTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var product = frame?.BindingContext as Product;
        if (product != null)
        {
            await Navigation.PushAsync(new ProductInfoPage(product ));
        }
    }

    private async void OnFeaturedTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var product = frame?.BindingContext as Product;
        if (product != null)
        {
            await Navigation.PushAsync(new ProductInfoPage(product));
        }
    }

    private async void OnAdTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var ad = frame?.BindingContext as Ad;
        if (ad != null)
        {
            await Navigation.PushAsync(new AdInfoPage(ad.ImageUrl,ad.Title,ad.Content));
        }
    }

    private async void RefreshButton_Clicked(object sender, EventArgs e)
    {
        // Show refresh indicator
        if (sender is Frame refreshFrame)
        {
            refreshFrame.IsVisible = false;
        }

        // Refresh the page content
        await LoadProducts();
        await LoadFeaturedProducts();
        await LoadAdverts();

        // Hide refresh indicator after a short delay
        await Task.Delay(1000);
        if (sender is Frame refreshFrame2)
        {
            refreshFrame2.IsVisible = true;
        }
    }
    public bool IsRefreshing;
    private async Task RefreshData()
    {
        _currentPage = 1;
        Products.Clear();
        IsRefreshing = true;
        await LoadInitialData();
       
        // Hide the refresh spinner after a short delay
        await Task.Delay(1000);
        
       
        IsRefreshing = false;
         
    }

    private async Task LoadMoreProducts()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            _currentPage++;
            await LoadProducts();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadInitialData()
    {
        await LoadProducts();
        await LoadFeaturedProducts();
        await LoadAdverts();
    }

    private async Task LoadProducts()
    {
        var products = await _supabaseService.GetProducts(_currentCategory, _currentPage, PageSize);
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
        ProductsCollectionView.ItemsSource = Products;
    }

    private async Task LoadFeaturedProducts()
    {
        var featured = await _supabaseService.GetFeaturedProducts();
        FeaturedProducts.Clear();
        foreach (var product in featured)
        {
            FeaturedProducts.Add(product);
        }
        FeaturedCollectionView.ItemsSource = FeaturedProducts;
    }

    private async Task LoadAdverts()
    {
        var adverts = await _supabaseService.GetActiveAds();
        Adverts.Clear();
        foreach (var ad in adverts)
        {
            Adverts.Add(ad);
        }
        AdsCarouselView.ItemsSource = Adverts;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        scrollTimer = new System.Timers.Timer(10000);
        scrollTimer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (AdsCarouselView.ItemsSource is IList adverts && adverts.Count > 0)
                {
                    currentIndex = (currentIndex + 1) % adverts.Count;
                    AdsCarouselView.ScrollTo(currentIndex, position: ScrollToPosition.Center, animate: true);
                }
            });
        };
        scrollTimer.Start();
        _isRunning = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        scrollTimer?.Stop();
        scrollTimer?.Dispose();
        _isRunning = false;
    }

    [RelayCommand]
    private async void OnCategorySelected(string category)
    {
        _currentCategory = category;
        _currentPage = 1;
        Products.Clear();
        await LoadProducts();
    }

    private async void OnCategoryButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            // Reset all buttons to default state
            foreach (var child in CategoryButtonsLayout.Children)
            {
                if (child is Button button)
                {
                    button.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Light 
                        ? (Color)Application.Current.Resources["SurfaceColor_Light"]
                        : (Color)Application.Current.Resources["SurfaceColor_Dark"];
                    button.TextColor = Application.Current.RequestedTheme == AppTheme.Light 
                        ? (Color)Application.Current.Resources["TextColor_Light"]
                        : (Color)Application.Current.Resources["TextColor_Dark"];
                }
            }

            // Set the clicked button to active state
            clickedButton.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Light 
                ? (Color)Application.Current.Resources["PrimaryColor_Light"]
                : (Color)Application.Current.Resources["PrimaryColor_Dark"];
            clickedButton.TextColor = Application.Current.RequestedTheme == AppTheme.Light 
                ? (Color)Application.Current.Resources["BackgroundColor_Light"]
                : (Color)Application.Current.Resources["BackgroundColor_Dark"];

            // Update current category and refresh products
            _currentCategory = clickedButton.Text.ToLower().Replace(" ", "_");
            _currentPage = 1;
            Products.Clear();
            await LoadProducts();
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchPage(  _authService,_supabaseService));
    }

    private async void LoadUserDetails()
    {
        try
        {
            var userEmail = Preferences.Get("UserEmail", string.Empty);
            if (!string.IsNullOrEmpty(userEmail))
            {
                _currentUser = await _supabaseService.GetUserByEmail(userEmail);
                if (_currentUser != null)
                {
                    // Update greeting
                    if (this.FindByName("HomePageGreeting") is Label greetingLabel)
                    {
                        greetingLabel.Text = $"Hello, {_currentUser.username}";
                    }

                    // Update avatar
                    if (this.FindByName("avatar_button") is ImageButton avatarButton)
                    {
                        if (!string.IsNullOrEmpty(_currentUser.avatar))
                        {
                            avatarButton.Source = _currentUser.avatar;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle error silently or show a message
            Console.WriteLine($"Error loading user details: {ex.Message}");
        }
    }
}
