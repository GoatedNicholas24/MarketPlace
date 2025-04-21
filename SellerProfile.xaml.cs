using System.Collections.ObjectModel;

namespace MarketPlace;

public partial class SellerProfile : ContentPage
{
	public Product _product;
    private readonly Supabase.Client _supabaseClient;

    public string SellerName { get; private set; }
    public string SellerAvatarUrl { get; private set; }
    public string Email { get; private set; }
    public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
    public SellerProfile(Product product)
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        _product = product;
        LoadSellerProfileAsync(product.Email);
        sellerName.Text = product.SellerName;
        sellerAvatar.Source = product.SellerAvatarUrl;
        sellerEmail.Text = product.Email;

    }
    private async void LoadSellerProfileAsync(string sellerEmail)
    {

        var response = await _supabaseClient.From<Product>().Where(e => e.Email == sellerEmail).Get();
        if(response.Models.Count > 0) {
            foreach (var product in response.Models)
            {
                Products.Add(product);
            }
            ProductsView.ItemsSource = Products;
                }
       
    }
    private async void OnProductTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var product = frame?.BindingContext as Product;

        if (product != null)
        {
            Preferences.Set("ProductId", product.ProductId);
            // await Shell.Current.GoToAsync("//ProductInfoPage");
            // Application.Current.MainPage = new ProductInfoPage(product); // Navigate to details page
            await Navigation.PushAsync(new ProductInfoPage(product));
        }
    }
}