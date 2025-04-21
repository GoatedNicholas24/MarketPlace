using System.Collections.ObjectModel;

namespace MarketPlace;

public partial class ProductPreviewPage : ContentPage
{
    private ObservableCollection<Product> previewModel { get; set; } = new();
    public ProductPreviewPage(Product product)
    {
        InitializeComponent();
        previewModel.Add(product);
        ProductPreviewCollectionView.ItemsSource = previewModel;
    }
    private async void ClosePreviewPopup(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
	
}