// DebugButtonHandler: デバッグボタンのクリックイベントを処理
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.UI
{
    /// <summary>
    /// デバッグボタンのクリックイベントを処理するハンドラー
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DebugButtonHandler : MonoBehaviour
    {
        [HideInInspector]
        public string methodName;

        private Button button;
        private GothicDebugMenu debugMenu;

        private void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            // GothicDebugMenuを探す
            debugMenu = GetComponentInParent<Canvas>()?.GetComponent<GothicDebugMenu>();
            if (debugMenu == null)
            {
                debugMenu = FindFirstObjectByType<GothicDebugMenu>();
            }
        }

        private void OnButtonClick()
        {
            if (debugMenu != null && !string.IsNullOrEmpty(methodName))
            {
                debugMenu.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning($"[DebugButtonHandler] Cannot invoke method: {methodName}");
            }
        }
    }
}
