// FantasyMiniMap: ミニマップ表示
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Project.Core.UI.Components
{
    /// <summary>
    /// ファンタジーミニマップ
    /// Phase 2実装: AAA Game Screen FantasyMiniMap.tsxを参考
    /// </summary>
    public class FantasyMiniMap
    {
        private readonly VisualElement root;
        private readonly VisualElement mapDisplay;
        private readonly Label locationLabel;
        private readonly Dictionary<string, VisualElement> icons;

        public FantasyMiniMap(VisualElement root)
        {
            this.root = root;

            if (root == null)
            {
                Debug.LogError("[FantasyMiniMap] Root element is null!");
                return;
            }

            mapDisplay = root.Q("map-display");
            locationLabel = root.Q<Label>("location-label");
            icons = new Dictionary<string, VisualElement>();
        }

        /// <summary>
        /// 現在地名を設定
        /// </summary>
        public void SetLocation(string locationName)
        {
            if (locationLabel != null)
            {
                locationLabel.text = locationName;
            }
        }

        /// <summary>
        /// アイコンを更新（追加または位置更新）
        /// </summary>
        public void UpdateIcon(string id, Vector2 normalizedPosition, IconType type)
        {
            if (mapDisplay == null) return;

            if (!icons.TryGetValue(id, out var icon))
            {
                icon = CreateIcon(type);
                mapDisplay.Add(icon);
                icons[id] = icon;
            }

            // 正規化座標（0-1）をパーセントに変換
            icon.style.left = Length.Percent(normalizedPosition.x * 100f);
            icon.style.top = Length.Percent(normalizedPosition.y * 100f);
        }

        /// <summary>
        /// アイコンを削除
        /// </summary>
        public void RemoveIcon(string id)
        {
            if (icons.TryGetValue(id, out var icon))
            {
                mapDisplay.Remove(icon);
                icons.Remove(id);
            }
        }

        /// <summary>
        /// アイコンを作成
        /// </summary>
        private VisualElement CreateIcon(IconType type)
        {
            var icon = new VisualElement();
            icon.AddToClassList("minimap-icon");
            
            switch (type)
            {
                case IconType.PlayerLocal:
                    icon.AddToClassList("minimap-icon-player-local");
                    break;
                case IconType.Player:
                    icon.AddToClassList("minimap-icon-player");
                    break;
                case IconType.Enemy:
                    icon.AddToClassList("minimap-icon-enemy");
                    break;
                case IconType.EnemyElite:
                    icon.AddToClassList("minimap-icon-enemy-elite");
                    break;
            }
            
            return icon;
        }

        public enum IconType
        {
            PlayerLocal,
            Player,
            Enemy,
            EnemyElite
        }
    }
}
