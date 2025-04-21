using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.ObjectModel;



[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("id", false)]
    public string id { get; set; }

    [Column("username")]
    public string username { get; set; }

    [Column("phone")]
    public string phone { get; set; }

    [Column("email" )]  
    public string email { get; set; }

    [Column("location")]
    public string location { get; set; }
    [Column("avatar")]
    public string avatar { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
