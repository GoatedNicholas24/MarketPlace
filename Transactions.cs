 
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
 
 
    [Table("transactions")]  // Ensure this matches your Supabase table name
public class Transaction : BaseModel
{
    // [PrimaryKey("user_id", false)]
    // public string UserId { get; set; }// False means it's not auto-incremented


    [Column("date")]
    public string Date { get; set; }

    [Column("amount")]
    public string Amount { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("reference")]
    public string Reference { get; set; }





}





