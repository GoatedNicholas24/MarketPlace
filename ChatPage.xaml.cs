using System.Collections.ObjectModel;
using System.Collections.Generic;
using Supabase.Interfaces;
namespace MarketPlace;
public partial class  ChatPage : ContentPage
{
   
    private string _chatId;
    private string _sellermail;
    private string _buyermail;
    public ObservableCollection<Order> orders { get; set; } = new();
    public ObservableCollection<Message> messages { get; set; } = new();
    private Supabase.Client _supabaseClient;
    public ChatPage(string chatId,string sellerEmail,string buyerEmail)
    {
        InitializeComponent();
        _chatId = chatId;
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        _sellermail = sellerEmail;
        _buyermail = buyerEmail;

        
       

         
        
       
    }
    private async Task<bool> IsSeller()
    {
        var email = Preferences.Get("UserEmail", "");
        var result = await _supabaseClient.From<Seller>().Where(r => r.Email == email).Get();
        if (result == null && result.Models.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }


    }
    private async void LoadOrders()
    {
        var chat = await _supabaseClient.From<Chat>().Where(m => m.id == _chatId).Get();
        var buyerEmail = chat.Model?.BuyerEmail;
        var sellerEmail = chat.Model?.sellerEmail;
        var orderResp = await _supabaseClient.From<Order>().Where(x=> x.buyer_email == buyerEmail&& x.SellerEmail == sellerEmail).Get();
         foreach(var aa in orderResp?.Models)
        {
            if(aa.status == "processing"|| aa.status == "pending" ||aa.status == "sent")
            {
                await DisplayAlert("dnnu", "adding orders", "ok");
                orders.Add(aa);
            }
           
        }
       OrdersView.ItemsSource = orders;
    }
    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        var message = MessageEntry.Text;
        var SenderEmail = Preferences.Get("UserEmail", ""); // nicomah..

        if (string.IsNullOrEmpty(message))
            return;
        var receiver = "";
      if(_buyermail == SenderEmail)
        {
             receiver = _sellermail;

        }
      if(_sellermail == SenderEmail)
        {
            receiver = _buyermail;
        }
        // Create new message and insert into database
        var newMessage = new Message
        {
            message_id = Guid.NewGuid().ToString(),
            chatId = _chatId,
            senderEmail = SenderEmail,
            receiverEmail =  receiver,
            Content = message
        };

        newMessage.IsSentByCurrentUser = true;
        messages.Add(newMessage);
        var response = await _supabaseClient.From<Message>().Insert(newMessage);
        if (response != null)
        {
            await _supabaseClient
            .From<Notification>()
            .Insert(new Notification
            {
                id = Guid.NewGuid().ToString(),
                user_email = receiver,
                title = $"New Message: {Preferences.Get("UserName", SenderEmail)} ",
                content = TruncateString(message,35),
                is_read = "false",
                type = "message"
            });
        }

        // Clear the message entry and reload messages
        MessageEntry.Text = string.Empty;
        LoadMessages();
    }
    public static string TruncateString(string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || maxLength <= 0)
            return string.Empty;

        if (input.Length <= maxLength)
            return input;

        int truncatedLength = maxLength - suffix.Length;
        if (truncatedLength <= 0)
            return suffix.Substring(0, maxLength); // edge case: suffix longer than maxLength

        return input.Substring(0, truncatedLength) + suffix;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadOrders();
        LoadMessages();

    }

    private async void LoadMessages()
    {
        var currentUserEmail = Preferences.Get("UserEmail", "");
        var messagesResp = await _supabaseClient.From<Message>()
            .Where(m => m.chatId == _chatId)
            .Order(m => m.created_at, Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        messages.Clear();
        foreach (var message in messagesResp?.Models ?? new List<Message>())
        {
            message.IsSentByCurrentUser = message.senderEmail == currentUserEmail;
            messages.Add(message);
        }

        MessagesView.ItemsSource = messages;
        
        // Scroll to the bottom
        if (messages.Count > 0)
        {
            MessagesView.ScrollTo(messages.Count - 1, position: ScrollToPosition.End, animate: true);
        }
    }
    private bool IsSentByCurrentUser( Message mess)
    {
        var email = Preferences.Get("UserEmail", "");
        if(mess.senderEmail == email) { return true; }
        else { return false; }
    }

    
}
