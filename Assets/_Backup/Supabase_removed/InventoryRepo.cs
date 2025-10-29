// Supabase Inventory Repository
using System;
using System.Linq;
using System.Threading.Tasks;
using Project.Core.Online.Models;

namespace Project.Core.Online.Repositories
{
    public static class InventoryRepo
    {
        public static async Task<InventoryRow[]> GetMineAsync()
        {
            // スタブ: 取得機能は無効化
            await Task.CompletedTask;
            return Array.Empty<InventoryRow>();
        }

        public static async Task UpsertAsync(string itemId, int quantity)
        {
            // スタブ: 書き込み機能は無効化
            await Task.CompletedTask;
        }
    }
}

