using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("notifications")]
public class Notification : BaseModel
{
    [PrimaryKey("id", true)]
    public string id { get; set; }  // Unique Order ID

    [Column("user_email")]
    public string user_email { get; set; }

    [Column("type")]
    public string type { get; set; }
    [Column("title")]
    public string  title { get; set; }
    [Column("content")]
    public string content { get; set; }
    [Column("is_read")]
    public string is_read { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; }
    [Column("order_id")]
    public string order_id{ get; set; }


}
