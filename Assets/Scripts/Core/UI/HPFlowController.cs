// HPFlowController: GothicUIのFluid Fillレベルを制御
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.UI
{
    /// <summary>
    /// GothicUIのFluidシェーダーの_FillLevelプロパティを制御
    /// </summary>
    public class HPFlowController : MonoBehaviour
    {
        private Material _material;
        private Image _image;
        private bool _initialized = false;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized) return;

            _image = GetComponent<Image>();
            if (_image != null && _image.material != null)
            {
                // マテリアルインスタンスを作成
                _material = new Material(_image.material);
                _image.material = _material;
                _initialized = true;
                Debug.Log($"[HPFlowController] Material instance created for {gameObject.name}");
            }
        }

        /// <summary>
        /// Fillレベルを設定（0.0～1.0）
        /// </summary>
        public void SetValue(float value)
        {
            // 初期化されていない場合は初期化
            if (!_initialized)
            {
                Initialize();
            }

            if (_material != null && _material.HasProperty("_FillLevel"))
            {
                _material.SetFloat("_FillLevel", value);
            }
        }

        private void OnDestroy()
        {
            // マテリアルインスタンスを破棄
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
