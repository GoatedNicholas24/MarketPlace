using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("ads")]
public class Ad : BaseModel
{
    [PrimaryKey("ad_id", true)]
    public string AdId { get; set; }  // Unique Order ID

    [Column("name")]
    public string AdName{ get; set; }

    [Column("image")]
    public string ImageUrl { get; set; }

    [Column("display_title")]
    public string Title { get; set; }

    [Column("display_content")]
    public string Content { get; set; }

    // New properties to add
    [Column("link_url")]
    public string LinkUrl { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }


}
