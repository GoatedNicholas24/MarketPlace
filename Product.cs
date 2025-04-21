
 
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.ObjectModel;
using System.Linq;

[Table("products")]
public class Product : BaseModel
{
 
    [Column("product_id")]
    public string ProductId { get; set; }
    [Column("seller_name")]
    public string SellerName { get; set; }
    [Column("seller_avatar")]
    public string SellerAvatarUrl { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("category")]
    public string Category { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("negotiable")]
    public bool Negotiable { get; set; }

    [Column("stock_quantity")]
    public string StockQuantity { get; set; }

    [Column("delivery_option")]
    public string DeliveryOption { get; set; }

    [Column("condition")]
    public string Condition { get; set; }

    [Column("tags")]
    public string Tags { get; set; }

    [Column("images")]
    public string Images { get; set; } // Store image URLs as comma-separated values
    [Column("seller_email")]
    public string Email { get; set; }
    [Column("impressions")]
    public int Impressions { get; set; }


    [Column("quantity")]
    public int Quantity { get; set; }
    [Column("popularity_score")]
    public int PopularityScore { get; set; }

    [Column("delivery_fee")]
    public int DeliveryFee{ get; set; }
    // New properties to add
    [Column("is_featured")]
    public bool IsFeatured { get; set; }

    [Column("is_new")]
    public bool IsNew { get; set; }


    [Column("rating")] 
    public decimal Rating { get; set; }

    [Column("review_count")]
    public int ReviewCount { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

}

