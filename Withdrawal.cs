using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("withdrawals")]
public class Withdrawal : BaseModel
{
    [PrimaryKey("id", true)]
    public string id { get; set; }  // Unique Order ID

    [Column("user_email")]
    public string user_email { get; set; }

    [Column("amount")]
    public int amount { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; }

    [Column("withdrawal_fee")]
    public string Withdrawal_Fee { get; set; }


}
