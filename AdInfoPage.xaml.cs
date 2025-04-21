namespace MarketPlace;
public partial class AdInfoPage : ContentPage
{
   
    public AdInfoPage(string imageUrl,string Title,string content)
	{
		InitializeComponent();
        AdImage.Source = imageUrl;
        TitleLabel.Text = Title;
        DescriptionLabel.Text = content;
        // LoadReviews();
    }




}