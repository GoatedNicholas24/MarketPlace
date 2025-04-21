using Supabase.Gotrue.Mfa;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
 
 
    [Table("seller")]  // Ensure this matches your Supabase table name
    public class Seller : BaseModel
    {
       // [PrimaryKey("user_id", false)]
       // public string UserId { get; set; }// False means it's not auto-incremented


        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("primary_phone")]
        public string PrimaryPhone { get; set; }

        [Column("secondary_phone")]
        public string SecondaryPhone { get; set; }
        //Added
        [Column("business_name")]
        public string BusinessName { get; set; }
        //
        [Column("business_type")]
        public string BusinessType { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("items_sold")]
        public string ItemsSold { get; set; }
    

        [Column("image_url")]
        public string ImageUrl { get; set; }

        [Column("business_description")]
        public string BusinessDescription { get; set; }

        [Column("approved")]
        public bool Approved { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }
        [Column("longitude")]
        public double Longitude { get; set; }
    [Column("wallet_pin")]
    public string WalletPin { get; set; }



}
    
 
 
           
            