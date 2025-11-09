// SimpleThirdPersonCamera: シンプルな3人称カメラ
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.Player
{
    /// <summary>
    /// シンプルな3人称カメラ（マウスで回転、ホイールでズーム）
    /// </summary>
    public class SimpleThirdPersonCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target; // プレイヤー

        [Header("Camera Settings")]
        [SerializeField] private float distance = 5.0f;
        [SerializeField] private float height = 2.0f;
        [SerializeField] private float rotationSpeed = 5.0f;
        [SerializeField] private float zoomSpeed = 2.0f;
        [SerializeField] private float minDistance = 2.0f;
        [SerializeField] private float maxDistance = 10.0f;

        [Header("Rotation Limits")]
        [SerializeField] private float minVerticalAngle = -20f;
        [SerializeField] private float maxVerticalAngle = 60f;

        private float _currentX = 0f;
        private float _currentY = 20f;
        private float _currentDistance;

        private void Start()
        {
            _currentDistance = distance;

            // ターゲットが設定されていない場合、Playerタグを探す
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("[SimpleThirdPersonCamera] Target set to Player");
                }
                else
                {
                    Debug.LogWarning("[SimpleThirdPersonCamera] No target found! Please assign a target or add 'Player' tag.");
                }
            }

            // カメラをPerspectiveに設定
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = false;
                cam.fieldOfView = 60f;
                Debug.Log("[SimpleThirdPersonCamera] Camera set to Perspective mode");
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // マウス右クリックでカメラ回転
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                _currentX += mouseDelta.x * rotationSpeed * Time.deltaTime;
                _currentY -= mouseDelta.y * rotationSpeed * Time.deltaTime;
                _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);
            }

            // マウスホイールでズーム
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                _currentDistance -= scroll * zoomSpeed * 0.01f;
                _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);
            }

            // カメラの位置を計算
            Quaternion rotation = Quaternion.Euler(_currentY, _currentX, 0);
            Vector3 offset = rotation * new Vector3(0, height, -_currentDistance);
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * height);
        }

        /// <summary>
        /// ターゲットを設定
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// カメラの距離を設定
        /// </summary>
        public void SetDistance(float newDistance)
        {
            _currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }
    }
}
