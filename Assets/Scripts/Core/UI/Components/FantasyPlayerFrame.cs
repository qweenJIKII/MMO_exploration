// FantasyPlayerFrame: HP/MP Orb + スキルバー表示
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Core.UI.Components
{
    /// <summary>
    /// プレイヤーフレーム（HP/MP Orb + スキルバー）
    /// Phase 2実装: AAA Game Screen FantasyPlayerFrame.tsxを参考
    /// </summary>
    public class FantasyPlayerFrame
    {
        private readonly VisualElement root;
        private readonly VisualElement healthOrbFill;
        private readonly VisualElement manaOrbFill;
        private readonly Label healthText;
        private readonly Label manaText;
        private readonly VisualElement skillBar;

        private float currentHealth;
        private float maxHealth;
        private float currentMana;
        private float maxMana;
        private int currentLevel;

        public FantasyPlayerFrame(VisualElement root)
        {
            this.root = root;

            if (root == null)
            {
                Debug.LogError("[FantasyPlayerFrame] Root element is null!");
                return;
            }

            // 要素取得
            healthOrbFill = root.Q("health-orb-fill");
            manaOrbFill = root.Q("mana-orb-fill");
            healthText = root.Q<Label>("health-text");
            manaText = root.Q<Label>("mana-text");
            skillBar = root.Q("skill-bar");

            // スキルスロット初期化
            InitializeSkills();
        }

        /// <summary>
        /// HPを更新
        /// </summary>
        public void UpdateHealth(float current, float max)
        {
            currentHealth = current;
            maxHealth = max;

            float percent = Mathf.Clamp01(current / max);
            
            // Orb塗りつぶし（下から上へ）
            if (healthOrbFill != null)
            {
                healthOrbFill.style.height = Length.Percent(percent * 100f);
            }

            // テキスト更新
            if (healthText != null)
            {
                healthText.text = $"{current:F0}";
            }
        }

        /// <summary>
        /// MPを更新
        /// </summary>
        public void UpdateMana(float current, float max)
        {
            currentMana = current;
            maxMana = max;

            float percent = Mathf.Clamp01(current / max);
            
            // Orb塗りつぶし（下から上へ）
            if (manaOrbFill != null)
            {
                manaOrbFill.style.height = Length.Percent(percent * 100f);
            }

            // テキスト更新
            if (manaText != null)
            {
                manaText.text = $"{current:F0}";
            }
        }

        /// <summary>
        /// スタミナを更新（Phase 5で拡張予定）
        /// </summary>
        public void UpdateStamina(float current, float max)
        {
            // TODO: スタミナバー実装
        }

        /// <summary>
        /// レベルを設定
        /// </summary>
        public void SetLevel(int level)
        {
            currentLevel = level;
            // TODO: レベル表示UI実装
        }

        /// <summary>
        /// 経験値を更新
        /// </summary>
        public void UpdateExperience(float current, float required)
        {
            // TODO: 経験値バー実装
        }

        /// <summary>
        /// スキルスロットを初期化（8個）
        /// </summary>
        private void InitializeSkills()
        {
            if (skillBar == null) return;

            for (int i = 0; i < 8; i++)
            {
                var skillSlot = CreateSkillSlot(i + 1);
                skillBar.Add(skillSlot);
            }
        }

        /// <summary>
        /// スキルスロットを作成
        /// </summary>
        private VisualElement CreateSkillSlot(int slotNumber)
        {
            var slot = new VisualElement();
            slot.AddToClassList("skill-slot");
            
            // ホットキー表示
            var hotkey = new Label(slotNumber.ToString());
            hotkey.AddToClassList("skill-hotkey");
            slot.Add(hotkey);

            // クールダウンオーバーレイ
            var cooldownOverlay = new VisualElement();
            cooldownOverlay.AddToClassList("skill-cooldown-overlay");
            cooldownOverlay.style.display = DisplayStyle.None;
            slot.Add(cooldownOverlay);

            return slot;
        }
    }
}
