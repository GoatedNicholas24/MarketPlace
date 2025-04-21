using System;
using Supabase;

namespace MarketPlace.Models;

public class Chat : SupabaseModel
{
    public string id { get; set; }
    public string BuyerEmail { get; set; }
    public string sellerEmail { get; set; }
    public string SenderName { get; set; }
    public string senderAvatar { get; set; }
    public string LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public bool HasUnreadMessages { get; set; }
    public int UnreadCount { get; set; }
} 