 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Supabase;

 

namespace MarketPlace 
{
    public partial class TemporaryChatDetailsPage : ContentPage
    {
        private Supabase.Client _supabaseClient;
        private ObservableCollection<Message> _messages;
        private TemporaryConversation _chat;

        public TemporaryChatDetailsPage(TemporaryConversation convo)
        {
            InitializeComponent();
            _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "YOUR_SUPABASE_API_KEY");
            _messages = new ObservableCollection<Message>();
            _chat = convo;

            // Bind the conversation to the page title
            Title = $"Chat with {_chat.SellerName}";

            // Set the items source for the CollectionView
            ChatMessagesView.ItemsSource = _messages;

            LoadMessages();
        }

        // Load the messages of the selected conversation from the database
        private async void LoadMessages()
        {
            while (true)
            {
                Task.Delay(6000);
                var email = Preferences.Get("UserEmail", "null");

                // Fetch messages related to this specific conversation
                var result = await _supabaseClient
                    .From<Message>()
                    .Where(x => (x.senderEmail == _chat.SellerEmail && x.receiverEmail == email) ||
                                (x.receiverEmail == _chat.SellerEmail && x.senderEmail == email))
                    .Get();

                if (result != null && result.Models.Count > 0)
                {
                    foreach (var message in result.Models)
                    {
                        _messages.Add(message);
                    }
                }
            }
            
        }

        // Send a new message
        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            string messageContent = MessageEntry.Text?.Trim();
            if (string.IsNullOrEmpty(messageContent))
                return;

            var newMessage = new Message
            {
                senderEmail = Preferences.Get("UserEmail", "null"),
                receiverEmail = _chat.SellerEmail,
                senderName = Preferences.Get("UserName", "User"),
                receiverName = _chat.SellerName,
                Content = messageContent,
                status = "sent",
                created_at = DateTime.UtcNow,
                type = "text"
            };

            // Save the message in the database
            var result = await _supabaseClient.From<Message>().Insert(newMessage);
            if (result != null)
            {
                // Add the message to the collection and clear the input field
                _messages.Add(newMessage);
                MessageEntry.Text = string.Empty;
                await _supabaseClient
         .From<Notification>()
         .Insert(new Notification
         {
             id = Guid.NewGuid().ToString(),
             user_email = _chat.SellerEmail,
             title = $"New Message: {_chat.SellerName}",
             content = TruncateString(messageContent,40),
             order_id ="",
             type = "order_placed"
         });
            }
            else
            {
                await DisplayAlert("Error", "Could not send message. Please try again.", "OK");
            }
        }
        public string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input; // Return original string if it's already shorter or equal to maxLength
            }

            return input.Substring(0, maxLength); // Truncate the string
        }

        // Delete a message
        private async void DeleteMessage_Clicked(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var messageToDelete = menuItem?.CommandParameter as Message;

            if (messageToDelete == null)
                return;

            // Delete the message from the database
            var result =  _supabaseClient.From<Message>().Where(x => x.message_id == messageToDelete.message_id).Delete();
            if (result != null)
            {
                _messages.Remove(messageToDelete);
            }
            else
            {
                await DisplayAlert("Error", "Could not delete message. Please try again.", "OK");
            }
        }
    }
   
    public class ChatPreview
    {
        public string OrderReference { get; set; }
        public string SellerName { get; set; }
        public string LastMessagePreview { get; set; }

        // Optional: used when opening full chat
        public List<Message> AllMessages { get; set; }
    }

}
