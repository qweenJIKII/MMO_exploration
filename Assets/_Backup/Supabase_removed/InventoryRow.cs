// Supabase Postgrest モデル: inventory
using Postgrest.Models;
using Postgrest.Attributes;
using System;

namespace Project.Core.Online.Models
{
    [Table("inventory")]
    public class InventoryRow : BaseModel
    {
        [PrimaryKey("id")] public Guid Id { get; set; }
        [Column("user_id")] public Guid UserId { get; set; }
        [Column("item_id")] public string ItemId { get; set; }
        [Column("quantity")] public int Quantity { get; set; }
        [Column("updated_at")] public DateTime UpdatedAt { get; set; }
    }
}
