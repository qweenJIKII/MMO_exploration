// UIカラーパレット: ファンタジーMMOテーマのカラー定義
using UnityEngine;

namespace Project.Core.UI.Theme
{
    /// <summary>
    /// ファンタジーMMO UIカラーパレット
    /// AAA Game Screenのデザインを参考
    /// </summary>
    public static class UIColorPalette
    {
        // Amber/Gold アクセント
        public static readonly Color Amber50 = new Color(1f, 0.984f, 0.941f, 1f);      // #fffef0
        public static readonly Color Amber300 = new Color(0.988f, 0.835f, 0.502f, 1f); // #fcd34d
        public static readonly Color Amber400 = new Color(0.984f, 0.741f, 0.251f, 1f); // #fbbf24
        public static readonly Color Amber600 = new Color(0.851f, 0.467f, 0.024f, 1f); // #d97706
        public static readonly Color Amber700 = new Color(0.706f, 0.345f, 0.012f, 1f); // #b45309
        public static readonly Color Amber800 = new Color(0.573f, 0.251f, 0.055f, 1f); // #92400e
        public static readonly Color Amber900 = new Color(0.471f, 0.188f, 0.051f, 1f); // #78300d

        // Stone 背景
        public static readonly Color Stone800 = new Color(0.161f, 0.141f, 0.125f, 1f); // #292420
        public static readonly Color Stone900 = new Color(0.110f, 0.098f, 0.090f, 1f); // #1c1917
        public static readonly Color Stone950 = new Color(0.051f, 0.047f, 0.043f, 1f); // #0d0c0b

        // Health（赤）
        public static readonly Color Red100 = new Color(0.996f, 0.886f, 0.886f, 1f);   // #fee2e2
        public static readonly Color Red500 = new Color(0.937f, 0.267f, 0.267f, 1f);   // #ef4444
        public static readonly Color Red600 = new Color(0.863f, 0.149f, 0.149f, 1f);   // #dc2626
        public static readonly Color Red700 = new Color(0.733f, 0.114f, 0.114f, 1f);   // #bb1d1d

        // Mana（青）
        public static readonly Color Blue100 = new Color(0.859f, 0.914f, 0.996f, 1f);  // #dbeafe
        public static readonly Color Blue500 = new Color(0.231f, 0.510f, 0.965f, 1f);  // #3b82f6
        public static readonly Color Blue600 = new Color(0.145f, 0.408f, 0.878f, 1f);  // #2563eb
        public static readonly Color Blue700 = new Color(0.114f, 0.227f, 0.541f, 1f);  // #1d3a8a

        // Status
        public static readonly Color Green400 = new Color(0.294f, 0.867f, 0.502f, 1f); // #4bdd80
        public static readonly Color Emerald600 = new Color(0.031f, 0.596f, 0.325f, 1f); // #059669
        public static readonly Color Purple500 = new Color(0.627f, 0.282f, 0.855f, 1f); // #a047da
        public static readonly Color Cyan400 = new Color(0.133f, 0.867f, 0.867f, 1f);  // #22d3ee

        // Text
        public static readonly Color TextPrimary = new Color(0.839f, 0.827f, 0.820f, 1f);    // #d6d3d1
        public static readonly Color TextSecondary = new Color(0.471f, 0.443f, 0.424f, 1f);  // #78716c
        public static readonly Color TextDisabled = new Color(0.292f, 0.275f, 0.263f, 1f);   // #4a4643

        /// <summary>
        /// 透明度を適用したカラーを取得
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// HP用グラデーション（開始色）
        /// </summary>
        public static Color HealthGradientStart => Red700;

        /// <summary>
        /// HP用グラデーション（終了色）
        /// </summary>
        public static Color HealthGradientEnd => Red500;

        /// <summary>
        /// MP用グラデーション（開始色）
        /// </summary>
        public static Color ManaGradientStart => Blue700;

        /// <summary>
        /// MP用グラデーション（終了色）
        /// </summary>
        public static Color ManaGradientEnd => Blue500;

        /// <summary>
        /// フレーム境界線の色
        /// </summary>
        public static Color FrameBorder => WithAlpha(Amber600, 0.5f);

        /// <summary>
        /// フレーム背景の色
        /// </summary>
        public static Color FrameBackground => WithAlpha(Stone900, 0.9f);
    }
}
