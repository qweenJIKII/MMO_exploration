namespace Project.Core.Online.Backend
{
    // クライアントとサーバ間で共有する簡易DTO（必要最小限）
    public sealed class ItemDto
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }

        public ItemDto() {}
        public ItemDto(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}
