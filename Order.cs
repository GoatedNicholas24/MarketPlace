using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("orders")]
public class Order : BaseModel
{
    [PrimaryKey("reference", true)]
    public string reference { get; set; }  // Unique Order ID
    

    [Column("buyer_name")]
    public string buyer_name { get; set; }

    [Column("delivery_option")]
    public string deliveryOption { get; set; }

    [Column("payment_option")]
    public string paymentOption { get; set; }

    [Column("buyer_Id")]
    public string BuyerId { get; set; }

    [Column("buyer_email")]
    public string buyer_email { get; set; }

    [Column("buyer_phone")]
    public string buyer_phone { get; set; }
    
    

    [Column("seller_email")]
    public string SellerEmail { get; set; }  // Foreign Key to Sellers
    
    [Column("product_id")]
    public string ProductId { get; set; }  // Foreign Key to Products
    [Column("product_name")]
    public string ProductName { get; set; }

    [Column("quantity")]
    public string Quantity { get; set; }

    [Column("amount")]
    public string amount { get; set; }

    [Column("order_details")]
    public string order_details { get; set; }  // Paid, Unpaid

    [Column("payment_method")]
    public string PaymentMethod { get; set; }  // Cash, EasyPay, etc.

    [Column("delivery_address")]
    public string delivery_address { get; set; }

    [Column("created_at")]
    public string created_at { get; set; }

    [Column("status")]
    public string status { get; set; }  // Pending, Shipped, Delivered
    [Column("delivery_fee")]
    public int DeliveryFee { get; set; }

}
