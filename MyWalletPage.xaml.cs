using System.Collections.ObjectModel;

namespace MarketPlace;

public partial class MyWalletPage : ContentPage
{
    private Supabase.Client _supabaseClient;
    private ObservableCollection<Transaction> Transactions { get; set; } = new();
    public MyWalletPage()
	{
		InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        LoadWalletInfo();
        LoadTrans();
    }
    private async void LoadWalletInfo()
    {
        var email = Preferences.Get("UserEmail", "null");
        if (_supabaseClient != null)
        {
            var response = await Task.Run(() =>  _supabaseClient.From<Wallet>().Where(x => x.user_email == email).Get());
            if (response != null)
            {
                var balance = response.Model.balance.ToString();
                BalanceLabel.Text = $"UGX {balance}";


            }
        }
       
    }

    private async void LoadTrans()
    {
        var email = Preferences.Get("UserEmail", "null");
        var result = await Task.Run(() =>   _supabaseClient.From<Order>().Where(x => x.SellerEmail == email && x.status == "received").Get());
        if (result != null && result.Models.Count > 0)
        {
            foreach (var sale in result.Models)
            {
                Transaction transaction = new Transaction();
                transaction.Reference = sale.reference;
                transaction.Amount = "+" + sale.amount;
                transaction.Date = sale.created_at;
                transaction.Type = "sale";
                Transactions.Add(transaction);
            }
        }
       var result1 = await Task.Run(() => _supabaseClient.From<Withdrawal>().Where(x => x.user_email == email).Get());
       if (result1 != null && result1.Models.Count > 0)
        {
            foreach (var withdrawal in result1.Models)
            {
                Transaction transaction = new Transaction();
                transaction.Reference = withdrawal.id;
                transaction.Amount = "-" + withdrawal.amount.ToString();
                transaction.Type = "withdraw";
                transaction.Date = withdrawal.CreatedAt;
                Transactions.Add(transaction);

            }
        }
        await DisplayAlert("Hey", "Transactions loaded", "Ok");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TransactionCollectionView.ItemsSource = Transactions;
        });
    }
    private async void WithdrawalButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new WithdrawalPage());
    }
}