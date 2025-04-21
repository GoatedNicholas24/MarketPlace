using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.Communication;

namespace MarketPlace;

public partial class WalletPage : ContentPage
{
	private Supabase.Client _supabaseClient;
	private ObservableCollection<Transaction> _transactions;

	public WalletPage()
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
		LoadWalletInfo();
    }
    private async void LoadWalletInfo()
    {
        var email = Preferences.Get("UserEmail", "null");
        if (_supabaseClient != null)
        {
            var response = await _supabaseClient.From<Wallet>().Where(x => x.user_email == email).Get();
            if (response != null)
            {
                var balance = response.Model.balance.ToString();
                BalanceLabel.Text = $"UGX {balance}";


            }
        }
        LoadTrans();
    }
	 
    private async void LoadTrans()
	{
        var email = Preferences.Get("UserEmail", "null");
        var result = await _supabaseClient.From<Order>().Where(x => x.SellerEmail == email && x.status == "received").Get();
        if (result != null && result.Models.Count>0)
        {
            foreach (var sale in result.Models)
            {
                Transaction transaction = new Transaction();
                transaction.Reference = sale.reference;
                transaction.Amount = "+" + sale.amount;
                transaction.Date = sale.created_at;
                transaction.Type = "sale";
                _transactions.Add(transaction);
            }
        }
        var result1 = await _supabaseClient.From<Withdrawal>().Where(x => x.user_email == email).Get();
        if (result1 != null && result1.Models.Count > 0)
        {
            foreach (var withdrawal in result1.Models)
            {
                Transaction transaction = new Transaction();
                transaction.Reference = withdrawal.id;
                transaction.Amount = "-" + withdrawal.amount.ToString();
                transaction.Type = "withdraw";
                transaction.Date = withdrawal.CreatedAt;
                _transactions.Add(transaction);

            }
        }
        TransactionCollectionView.ItemsSource = _transactions;
    }
	private async void WithdrawalButtonClicked(object sender, EventArgs e)
	{
      await Navigation.PushAsync(new WithdrawalPage());
	}
}