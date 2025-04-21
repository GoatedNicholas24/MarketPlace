using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("wallets")]
public class Wallet : BaseModel
{
    [PrimaryKey("id", true)]
    public string id { get; set; }  // Unique Order ID

    [Column("user_email")]
    public string user_email { get; set; }

    [Column("balance")]
    public int balance { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; }


}
