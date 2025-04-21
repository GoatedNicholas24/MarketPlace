using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("cart")]
public class Cart : BaseModel
{
    [PrimaryKey("cart_id", true)]
    public string Id { get; set; }  // Unique Order ID

    [Column("user_email")]
    public string User_Email { get; set; }

    [Column("product_id")]
    public string Product_Id { get; set; }

    [Column("quantity")]
    public string Quantity { get; set; }
}
