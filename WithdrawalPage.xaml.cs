using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MarketPlace;

public partial class WithdrawalPage : ContentPage
{
	public Supabase.Client _supabaseClient;
	public WithdrawalPage()
	{
		InitializeComponent();
		_supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
	}
	private async void ProceedClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(AmountEntry.Text) || string.IsNullOrWhiteSpace(PhoneEntry.Text) || string.IsNullOrWhiteSpace(PINEntry.Text))
		{
			await DisplayAlert("Warning", "Please fill in all fields", "Ok");
		}

		var amountEntered = AmountEntry.Text.Trim();
		if (!int.TryParse(amountEntered, out int amount))
		{
			await DisplayAlert("error", "Enter valid amount", "Ok");
		}
		var pin = PINEntry.Text.Trim();
		var phone = PhoneEntry.Text.Trim();
		var email = Preferences.Get("UserEmail", "null");
		if (!IsValidPhoneNumber(phone))
		{
			await DisplayAlert("Invalid Phone Number", "Use the format 256712345678","Ok");
			return;
		}
		if (_supabaseClient != null)
		{
            var withdrawalFee = CalculateWithdrawalFee(amount);
            var totalRequired = amount + withdrawalFee;
            var result = await _supabaseClient.From<Wallet>().Where(x => x.user_email == email).Get();
			var response = await _supabaseClient.From<Seller>().Where(x => x.Email == email).Get();
            
           
            if (result != null && response != null) {
				var balance = result.Model.balance;
				var Wpin = response.Model.WalletPin.Trim();
                if (balance >= totalRequired && Wpin == pin && amount >= 2000)
                {
                    var answer = await  DisplayAlert("Withdrawal", $"Withdrawal of UGX {amount} will cost UGX {withdrawalFee}. Total deducted: UGX {totalRequired}.", "OK","Cancel");

                    // Proceed with withdrawal logic
                    if (answer)
                    { InitiateWithdrawal(phone, Guid.NewGuid().ToString(), amount,withdrawalFee); }
                    else
                    {
                        return;
                    }
                }
                if(balance < totalRequired)
                {
                    var shortfall = totalRequired - balance;
                    await DisplayAlert("Insufficient Balance", $"You need UGX {totalRequired}, but you only have UGX {balance}. Shortfall: UGX {shortfall}.", "OK");
                    return;
                    // Do not proceed
                }
                
				 
				if (Wpin != pin)
				{
					await DisplayAlert("Invalid!", "You entered a wrong pin. Try again", "Ok");
				}
				if (amount < 2000)
				{
					await DisplayAlert("Warning", "The minimum amount to be withdrawn is UGX 3000 ", "Ok");
				}

			}


		}
	}
    public class WithdrawalResponse
    {
        public int success { get; set; }
        public WithdrawalData data { get; set; }
    }
    public class WithdrawalData
    {
        public string status { get; set; }
        public string amount { get; set; }
        public string charge { get; set; }
        public string reference { get; set; }
    }
    private async Task CheckWithdrawalStatus(string Reference, int Amount, double fee  )
    {
        while (true)
        {

            await Task.Delay(20000); // Check every 5 seconds

            var payload = new
            {
                username = "64a52df432f2b748",
                password = "49b3629b4feea447",
                action = "mmstatus",
                reference = Reference
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

                    var easyPayResponse = JsonSerializer.Deserialize<WithdrawalResponse>(jsonResponse, options);
					 


                    if (easyPayResponse.success == 1 && easyPayResponse.data.status == "Success")
                    {
						var withd = new Withdrawal
						{
							id = Guid.NewGuid().ToString(),
							amount = Amount,
							user_email = Preferences.Get("UserEmail","null"),
							Withdrawal_Fee = fee.ToString()
						};

                        await _supabaseClient.From<Withdrawal>().Insert(withd);
						                       await _supabaseClient
            .From<Notification>()
            .Insert(new Notification
            {
                id = Guid.NewGuid().ToString(),
                user_email = Preferences.Get("UserEmail", "null"),
                title = "Withdrawal Success",
                content = $"You have a successfully withdrawn { Amount} with charges of{ fee}.",
                
                type = "withdraw_success"
            });
                        var respon = await _supabaseClient.From<Wallet>().Where(x => x.user_email == Preferences.Get("UserEmail", "null")).Get();
                        var currentBal = respon.Model.balance;
                        var newbal = currentBal - Amount - fee;
                        await _supabaseClient.From<Wallet>().Where(x => x.user_email == Preferences.Get("UserEmail", "null")).Set(x=> x.balance,newbal).Update();
                         

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
    double CalculateWithdrawalFee(double amount)
    {
        double fee;

        if (amount <= 2500) fee = 400;
        else if (amount <= 5000) fee = 440 + 0.03*amount;
        else if (amount <= 15000) fee = 600 + 0.03 * amount;
        else if (amount <= 30000) fee = 800 + 0.025 * amount;
        else if (amount <= 45000) fee = 1000 + 0.025 * amount;
        else if (amount <= 60000) fee = 1300 + 0.025 * amount;
        else if (amount <= 125000) fee = 1500 + 0.028*amount/1.5;
        else if (amount <= 250000) fee = 2000 + 0.028*amount/3;
        else if (amount <= 500000) fee = 5000 + 0.028 * amount/ 5;
        else if (amount <= 1000000) fee = 7000 + 0.028 * amount/10;
        else if (amount <= 2000000) fee = 9000 + 0.028 * amount/20;
        else if (amount <= 5000000) fee = 10000 + 0.028 * amount/20;
        else fee = 10000 + 0.028 * amount /20; // Default max fee

        double handlingFee = 200; // your platform fee
        return fee + handlingFee;
    }

    private async void InitiateWithdrawal(string phone, string reference, int amount, double fee)
	{
		var paymentRequest = new
		{
			username = "64a52df432f2b748",
			password = "49b3629b4feea447",
			action = "mmpayout",
			amount = amount,
			currency = "UGX",
			phone = phone,
			reference = reference

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
					await DisplayAlert("Payment Success", $"Amount: {paymentResponse.data.amount}\nReference: {paymentResponse.data.reference}", "OK");
					await CheckWithdrawalStatus(reference,amount,fee);
				}
				else
				{
					await DisplayAlert("Withdrawal Failed", "Could not initiate withdrawal. Please try again.", "OK");
				}
			}

		}
	}
    static bool IsValidPhoneNumber(string phoneNumber)
    {
        string pattern = @"^256\d{9}$";  // Ensures it starts with 256 and is followed by exactly 9 digits (total 12 digits)
        Regex regex = new Regex(pattern);
        return regex.IsMatch(phoneNumber);
    }
    public class EasyPayResponse
	{
		public Data data { get; set; }
		public int success { get; set; }
		 
	}
	public class Data
	{
		public string phone {get; set;}
		public string reference {get; set;}
		public string transactionId { get; set; }
		public string amount { get; set; }
		public string reason { get; set; }
	}
}
