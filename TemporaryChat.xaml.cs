using System.Collections.ObjectModel;
using System.Linq;

namespace MarketPlace;

public partial class TemporaryChat : ContentPage
{
    private Supabase.Client _supabaseClient;
    private  TemporaryConversation _conversations;
    private ObservableCollection<ChatPreview> _previews;

    public TemporaryChat()
    {
        InitializeComponent();
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0");
        _previews = new ObservableCollection<ChatPreview>();
        ChatMessagesView.ItemsSource = _previews;

        LoadTemporaryChats();
    }

    private async void LoadTemporaryChats()
    {
        var myEmail = Preferences.Get("UserEmail", "null");

        var messagesResult = await _supabaseClient
            .From<Message>()
            .Where(x => x.senderEmail == myEmail || x.receiverEmail == myEmail)
            .Get();

        if (messagesResult?.Models == null) return;

        var groupedChats = messagesResult.Models
            .GroupBy(m => new
            {
                OrderReference = m.reference, // Assume you have order ID in message_id
                PartnerEmail = m.senderEmail == myEmail ? m.receiverEmail : m.senderEmail,
                PartnerName = m.senderEmail == myEmail ? m.receiverName : m.senderName
            })
            .Select(g =>
            {
                var latest = g.OrderByDescending(m => m.created_at).First();
                return new ChatPreview
                {
                    OrderReference = g.Key.OrderReference,
                    SellerName = g.Key.PartnerName,
                    LastMessagePreview = latest.Content,
                    AllMessages = g.ToList()
                };
            });

        foreach (var preview in groupedChats)
            _previews.Add(preview);
    }

    private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;

        var selected = e.CurrentSelection.FirstOrDefault() as ChatPreview;
        if (selected == null) return;

        await Navigation.PushAsync(new TemporaryChatDetailsPage(_conversations));
        ChatMessagesView.SelectedItem = null;
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
public class TemporaryConversation
{
    public string OrderReference { get; set; }
    public string SellerName { get; set; }
    public string BuyerName { get; set; }
    public string SellerEmail { get; set; }
    public string BuyerEmail { get; set; }
    public string LastMessagePreview { get; set; }
}
