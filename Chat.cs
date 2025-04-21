using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("chats")]
public class Chat: BaseModel
{
    [PrimaryKey("id")]
    public string id { get; set; }  // Unique Order ID

    [Column("order_id")]
    public string order_id { get; set; }
    [Column("seller_email")]
    public string sellerEmail { get; set; }
    [Column("sender_avatar")]
    public string senderAvatar { get; set; }

    [Column("buyer_email")]
    public string BuyerEmail { get; set; }
    [Column("sender_name")]
    public string SenderName { get; set; }

    [Column("last_message")]
    public string LastMessage { get; set; }
   
    [Column("has_unread")]
    public bool HasUnreadMessages { get; set; }
    [Column("unread_count")]
    public int UnreadCount { get; set; }

    [Column("last_message_time")]
    public DateTime LastMessageTime { get; set; }


}
