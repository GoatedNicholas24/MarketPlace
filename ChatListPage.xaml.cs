using Supabase.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using MarketPlace.Services;
using Supabase;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Supabase.Postgrest;
 
using System.Threading.Tasks;

namespace MarketPlace;

public partial class ChatListPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly SupabaseService _supabaseService;
    public Supabase.Client _supabaseClient;
    public ObservableCollection<Chat> Chats { get; set; } = new();
    public ObservableCollection<Chat> FilteredChats { get; set; } = new();
    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand => new Command(async () => await LoadChats());

    public ChatListPage(AuthService authService, SupabaseService supabaseService)
    {
        InitializeComponent();
        _authService = authService;
        _supabaseService = supabaseService;
        var options = new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };
       
        _supabaseClient = new Supabase.Client("https://bbgpafulnowlgduckgie.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJiZ3BhZnVsbm93bGdkdWNrZ2llIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzgzMDE5NjIsImV4cCI6MjA1Mzg3Nzk2Mn0.Su5j3p4Lc1b-Wfskks6Ogh7IzSlsKTJZVy5jHNflHd0", options);
        
        // Subscribe to real-time notifications
        SubscribeToNotifications();
    }

    private async Task SubscribeToNotifications()
    {
        var email = Preferences.Get("UserEmail", "");

        while (true)
        {
            await Task.Delay(10000);
            var result = await _supabaseClient.From<Message>().Where(x => x.receiverEmail == email || x.senderEmail == email).Get();
            foreach(var mess in result.Models)
            {
                UpdateUnreadCount(mess);
            }
        }
    }
    
    private void UpdateUnreadCount(Message message)
    {
        var chat = Chats.FirstOrDefault(c => c.id == message.chatId);
        if (chat != null)
        {
            chat.HasUnreadMessages = true;
            chat.UnreadCount++;
            chat.LastMessage = TruncateString(message.Content, 30);
            chat.LastMessageTime = message.created_at;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadChats();
    }

    private async Task<bool> IsSeller()
    {
        var email = Preferences.Get("UserEmail", "");
        var result = await _supabaseClient.From<Seller>().Where(r => r.Email == email).Get();
        return result != null && result.Models.Count > 0;
    }

    private async Task LoadChats()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== Starting LoadChats ===");
            IsRefreshing = true;
            Chats.Clear();
            FilteredChats.Clear();

            var email = Preferences.Get("UserEmail", "");
            System.Diagnostics.Debug.WriteLine($"User email from preferences: {email}");
            
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("User email not found in preferences");
            }

            System.Diagnostics.Debug.WriteLine("Fetching chats from database...");
            var response = await _supabaseClient.From<Chat>()
                .Where(c => c.BuyerEmail == email || c.sellerEmail == email)
                .Get();

            System.Diagnostics.Debug.WriteLine($"Database response received. Models count: {response?.Models?.Count ?? 0}");

            var chats = response.Models;
            if (chats.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Checking if user is seller...");
                bool isSeller = await IsSeller();
                System.Diagnostics.Debug.WriteLine($"User is seller: {isSeller}");

                foreach (var chat in chats)
                {
                    try 
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing chat ID: {chat.id}");
                        
                        if (isSeller)
                        {
                            System.Diagnostics.Debug.WriteLine($"Fetching buyer info for email: {chat.BuyerEmail}");
                            var response1 = await _supabaseClient.From<User>()
                                .Where(m => m.email == chat.BuyerEmail)
                                .Get();
                            
                            if (response1?.Model == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Buyer user not found for email: {chat.BuyerEmail}");
                                continue;
                            }
                            
                            chat.senderAvatar = response1.Model.avatar;
                            chat.SenderName = response1.Model.username;
                            System.Diagnostics.Debug.WriteLine($"Buyer info set: {chat.SenderName}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Fetching seller info for email: {chat.sellerEmail}");
                            var response1 = await _supabaseClient.From<User>()
                                .Where(m => m.email == chat.sellerEmail)
                                .Get();
                            
                            if (response1?.Model == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Seller user not found for email: {chat.sellerEmail}");
                                continue;
                            }
                            
                            chat.senderAvatar = response1.Model.avatar;
                            chat.SenderName = response1.Model.username;
                            System.Diagnostics.Debug.WriteLine($"Seller info set: {chat.SenderName}");
                        }

                        System.Diagnostics.Debug.WriteLine("Fetching last message...");
                        var messageResp = await _supabaseClient.From<Message>()
                            .Where(m => m.chatId == chat.id)
                            .Order(m => m.created_at, Supabase.Postgrest.Constants.Ordering.Descending)
                            .Get();
                        
                        if (messageResp?.Models == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: No messages found for chat: {chat.id}");
                            continue;
                        }
                        
                        var lastMessage = messageResp.Models.FirstOrDefault();
                        if (lastMessage != null)
                        {
                            chat.LastMessage = TruncateString(lastMessage.Content, 30);
                            chat.LastMessageTime = lastMessage.created_at;
                            System.Diagnostics.Debug.WriteLine($"Last message set: {chat.LastMessage}");
                            
                            try
                            {
                                System.Diagnostics.Debug.WriteLine("Checking for unread messages...");
                                var unreadCount = await _supabaseClient.From<Message>()
                                    .Where(m => m.chatId == chat.id && 
                                              m.receiverEmail == email && 
                                              m.is_read == false)
                                    .Count(Supabase.Postgrest.Constants.CountType.Exact);
                                
                                chat.HasUnreadMessages = unreadCount > 0;
                                chat.UnreadCount = unreadCount;
                                System.Diagnostics.Debug.WriteLine($"Unread count: {unreadCount}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error getting unread count: {ex.Message}");
                                chat.HasUnreadMessages = false;
                                chat.UnreadCount = 0;
                            }
                        }

                        Chats.Add(chat);
                        System.Diagnostics.Debug.WriteLine($"Chat {chat.id} added to collection");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing chat {chat.id}: {ex.Message}\nStack trace: {ex.StackTrace}");
                        continue;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Total chats processed: {Chats.Count}");
                FilteredChats = new ObservableCollection<Chat>(Chats);
                ChatsCollectionView.ItemsSource = FilteredChats;
                System.Diagnostics.Debug.WriteLine("Chat list updated in UI");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No chats found for user");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadChats Error ===\nMessage: {ex.Message}\nStack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Failed to load chats: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
            System.Diagnostics.Debug.WriteLine("=== LoadChats Completed ===");
        }
    }

    public static string TruncateString(string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || maxLength <= 0)
            return string.Empty;

        if (input.Length <= maxLength)
            return input;

        int truncatedLength = maxLength - suffix.Length;
        if (truncatedLength <= 0)
            return suffix.Substring(0, maxLength);

        return input.Substring(0, truncatedLength) + suffix;
    }

    private async void OnChatTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;
        var chat = frame?.BindingContext as Chat;
        if (chat != null)
        {
            // Mark messages as read
            await MarkMessagesAsRead(chat.id);
            await Navigation.PushAsync(new ChatPage(chat.id, chat.sellerEmail, chat.BuyerEmail));
        }
    }

    private async Task MarkMessagesAsRead(string chatId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Marking messages as read for chat: {chatId}");
            var email = Preferences.Get("UserEmail", "");
            
            // First get the unread messages
            var unreadMessages = await _supabaseClient.From<Message>()
                .Where(m => m.chatId == chatId)
                .Where(m => m.receiverEmail == email)
                .Where(m => m.is_read == false)
                .Get();

            if (unreadMessages?.Models != null && unreadMessages.Models.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Found {unreadMessages.Models.Count} unread messages to mark as read");
                
                // Update each message individually
                foreach (var message in unreadMessages.Models)
                {
                    await _supabaseClient.From<Message>()
                        .Where(m => m.message_id == message.message_id)
                        .Set(m => m.is_read, true)
                        .Update();
                }
                
                System.Diagnostics.Debug.WriteLine("Messages marked as read successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No unread messages found to mark as read");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error marking messages as read: {ex.Message}\nStack trace: {ex.StackTrace}");
            // Don't throw the error to the user, just log it
        }
    }

    public void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredChats = new ObservableCollection<Chat>(Chats);
        }
        else
        {
            FilteredChats = new ObservableCollection<Chat>(
                Chats.Where(c => 
                    c.SenderName?.ToLower().Contains(searchText) == true ||
                    c.LastMessage?.ToLower().Contains(searchText) == true
                )
            );
        }
        ChatsCollectionView.ItemsSource = FilteredChats;
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage1(_supabaseService, _authService));
    }
    private async void RefreshButton_Clicked(object sender, EventArgs e)
    {
        await LoadChats();
    }
    
}
 
