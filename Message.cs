using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("messages")]
public class Message : BaseModel
{
    [PrimaryKey("message_id", true)]
    public string message_id { get; set; }  // Unique Order ID

    [Column("sender_name")]
    public string senderName { get; set; }
   


    [Column("chat_id")]
    public string chatId { get; set; }
    [Column("sender_email")]
    public string senderEmail { get; set; }

    [Column("receiver_name")]
    public string receiverName { get; set; }
    [Column("receiver_email")]
    public string receiverEmail { get; set; }

    [Column("content")]
    public string Content { get; set; }

    [Column("created_at")]
    public DateTime created_at { get; set; }
    [Column("is_read")]
    public bool is_read { get; set; }
    [Column("status")]
    public string status{ get; set; }
    [Column("reference")]
    public string reference { get; set; }
    [Column("type")]
    public string type { get; set; }
    [Column("is_current_user")]
    public bool IsSentByCurrentUser { get; set; }


}
