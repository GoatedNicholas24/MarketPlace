using System.Collections.ObjectModel;
using MarketPlace.Services; 

namespace MarketPlace;

public partial class SearchPage : ContentPage
{
    public string useremail;
    public decimal _unitPrice;
    public string _productId;
    public string _sellerEmail;
    public string _itemName;
    public string _username;
    public bool _isRunning;

    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    private ObservableCollection<Product> AllProducts = new();
    private ObservableCollection<Product> FilteredProducts = new();
    public Supabase.Client _supabaseClient;
    public ObservableCollection<Product> Products { get; set; } = new();
    public ObservableCollection<string> Categories { get; set; } = new();
    public SearchPage(AuthService authService, SupabaseService supabaseService)
    {
        InitializeComponent();
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,  // Ensures tokens refresh automatically
            AutoConnectRealtime = true
        };
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
        SearchBar.TextChanged += SearchBar_TextChanged;
        LoadProducts();
        _supabaseService = supabaseService;
        _authService = authService;
    }
    private async void RefreshButton_Clicked(object sender, EventArgs e)
    {
        LoadProducts();
         
    }
    private async void OnProductTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var product = frame?.BindingContext as Product;

        if (product != null)
        {
            //await Shell.Current.GoToAsync(nameof(ProductInfoPage), new Dictionary<string, object> { { "productId", product.ProductId } });// Navigate to details page
            await Navigation.PushAsync(new ProductInfoPage(product));
        }
    }
    private void PopulateCategories()
    {
        CategoriesLayout.Children.Clear();
        foreach (var category in Categories)
        {
            var btn = new Button
            {
                Text = category,
                BackgroundColor = category == SelectedCategory ? Colors.LightGreen : Colors.DarkGrey,
                TextColor = Colors.Black,
                CornerRadius = 10
            };
            btn.Clicked += (s, e) => FilterByCategory(category);
            CategoriesLayout.Children.Add(btn);
        }
    }
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage1(_supabaseService,_authService));
        //await Shell.Current.GoToAsync(nameof(HomePage1));
    }
    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_authService,_supabaseService));
        //await Shell.Current.GoToAsync(nameof(CartPage));
    }
    private async void OnSearchClicked(object sender, EventArgs e)
    {
         LoadProducts();
    }
    private string SelectedCategory = "All";
    private void FilterByCategory(string category)
    {
        SelectedCategory = category; // Store the selected category

        FilteredProducts.Clear();
        if (category == "All")
        {
            foreach (var product in Products)
                FilteredProducts.Add(product);
        }
        else
        {
            foreach (var product in Products.Where(p => p.Category == category))
                FilteredProducts.Add(product);
        }

        ProductsCollectionView.ItemsSource = FilteredProducts;
        PopulateCategories(); // Refresh button styles
    }

    private async void LoadProducts()
    { 

        try
        {

            var response = await _supabaseClient.From<Product>().Select("*").Get();
            if (response.Models.Count == 0)
            {
                await DisplayAlert("Error", "No products fetched", "Ok");
            }
            var productList = response.Models;

            Products.Clear();
            foreach (var product in productList)
            {


                Products.Add(product);
            }

            FilteredProducts = new ObservableCollection<Product>(Products);
            ProductsCollectionView.ItemsSource = FilteredProducts;



            // Load categories dynamically

            Categories.Clear();
            Categories.Add("All");
            foreach (var category in Products.Select(p => p.Category).Distinct())
            {
                Categories.Add(category);
            }
            PopulateCategories();

            // Maintain category selection after refresh
            if (SelectedCategory == "All")
                FilteredProducts = new ObservableCollection<Product>(Products);
            else
                FilterByCategory(SelectedCategory);

            ProductsCollectionView.ItemsSource = FilteredProducts;


        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

    }

  
    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower();
        if (searchText == null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredProducts = new ObservableCollection<Product>(Products);
        }
        else
        {
            FilteredProducts.Clear();
            foreach (var product in Products.Where(p =>
                p.Category.ToLower().Contains(searchText)||
                p.Name.ToLower().Contains(searchText)||
                p.Price.ToString().ToLower().Contains(searchText)||
                p.DeliveryOption.ToLower().Contains(searchText)||
                p.Description.ToLower().Contains(searchText)))
            {
                FilteredProducts.Add(product);
            }
        }
        ProductsCollectionView.ItemsSource = FilteredProducts;
    }
    private async void ChatIcon_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChatListPage(_authService,_supabaseService));
    }
    public async void HomeMenuButtonClicked(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new HomeMenuPage(_authService,_supabaseService
            ));
    }

    private async void OnAddToCart(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Product product)
        {
            try
            {
                var userEmail = Preferences.Get("UserEmail", string.Empty);
                if (string.IsNullOrEmpty(userEmail))
                {
                    await DisplayAlert("Error", "Please sign in to add items to cart", "OK");
                    return;
                }

                

                await _supabaseService.AddToCart (Preferences.Get("UserEmail", string.Empty),product.ProductId,1);
                await DisplayAlert("Success", "Item added to cart", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnAddToWishlist(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Product product)
        {
            try
            {
                var userEmail = Preferences.Get("UserEmail", string.Empty);
                if (string.IsNullOrEmpty(userEmail))
                {
                    await DisplayAlert("Error", "Please sign in to add items to wishlist", "OK");
                    return;
                }

               

                await _supabaseService.AddToWishlist(userEmail,product.ProductId);
                await DisplayAlert("Success", "Item added to wishlist", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
