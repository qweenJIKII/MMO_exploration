// カメラコントローラー: Cinemachineと連携したカメラ制御
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤー追従カメラの制御（Cinemachine使用）
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private Transform cameraTarget; // カメラが追従するターゲット

        [Header("Rotation Settings")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float gamepadSensitivity = 100.0f;
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;

        [Header("Zoom Settings")]
        [SerializeField] private float minDistance = 3.0f;
        [SerializeField] private float maxDistance = 10.0f;
        [SerializeField] private float zoomSpeed = 2.0f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference zoomAction;

        private float _currentDistance = 5.0f;
        private float _verticalAngle = 0f;
        private float _horizontalAngle = 0f;
        private CinemachineThirdPersonFollow _thirdPersonFollow;

        private void Awake()
        {
            // Virtual Cameraの設定を取得
            if (virtualCamera != null)
            {
                _thirdPersonFollow = virtualCamera.GetComponent<CinemachineThirdPersonFollow>();
                _currentDistance = (_thirdPersonFollow != null) ? _thirdPersonFollow.CameraDistance : 5.0f;
            }

            // カメラターゲットがない場合は自動生成
            if (cameraTarget == null)
            {
                GameObject targetObj = new GameObject("CameraTarget");
                cameraTarget = targetObj.transform;
                cameraTarget.SetParent(transform);
                cameraTarget.localPosition = new Vector3(0, 1.5f, 0); // プレイヤーの頭上あたり
            }

            // Virtual Cameraのターゲットを設定
            if (virtualCamera != null)
            {
                virtualCamera.Follow = cameraTarget;
                virtualCamera.LookAt = cameraTarget;
            }
        }

        private void OnEnable()
        {
            // Input Actionsを有効化
            if (lookAction != null)
                lookAction.action.Enable();
            if (zoomAction != null)
                zoomAction.action.Enable();
        }

        private void OnDisable()
        {
            // Input Actionsを無効化
            if (lookAction != null)
                lookAction.action.Disable();
            if (zoomAction != null)
                zoomAction.action.Disable();
        }

        private void LateUpdate()
        {
            // カメラ回転処理
            HandleRotation();

            // カメラズーム処理
            HandleZoom();
        }

        private void HandleRotation()
        {
            if (lookAction == null) return;

            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            // 入力デバイスに応じて感度を調整
            float sensitivity = mouseSensitivity;
            var mouse = Mouse.current;
            var gamepad = Gamepad.current;

            if (gamepad != null && lookInput.sqrMagnitude > 0.01f)
            {
                // ゲームパッドの場合
                sensitivity = gamepadSensitivity * Time.deltaTime;
            }
            else if (mouse != null)
            {
                // マウスの場合（デルタ値なのでTime.deltaTimeは不要）
                sensitivity = mouseSensitivity;
            }

            // 水平・垂直角度を更新
            _horizontalAngle += lookInput.x * sensitivity;
            _verticalAngle -= lookInput.y * sensitivity; // Y軸は反転

            // 垂直角度を制限
            _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);

            // カメラターゲットの回転を更新
            if (cameraTarget != null)
            {
                cameraTarget.rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);
            }
        }

        private void HandleZoom()
        {
            if (zoomAction == null || _thirdPersonFollow == null) return;

            float zoomInput = zoomAction.action.ReadValue<float>();

            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                // ズーム量を更新
                _currentDistance -= zoomInput * zoomSpeed * Time.deltaTime;
                _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);

                // Cinemachineの距離を更新
                _thirdPersonFollow.CameraDistance = _currentDistance;
            }
        }

        /// <summary>
        /// カメラの角度を設定
        /// </summary>
        public void SetRotation(float horizontal, float vertical)
        {
            _horizontalAngle = horizontal;
            _verticalAngle = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);

            if (cameraTarget != null)
            {
                cameraTarget.rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);
            }
        }

        /// <summary>
        /// カメラの距離を設定
        /// </summary>
        public void SetDistance(float distance)
        {
            _currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
            
            if (_thirdPersonFollow != null)
            {
                _thirdPersonFollow.CameraDistance = _currentDistance;
            }
        }

        /// <summary>
        /// Virtual Cameraを設定
        /// </summary>
        public void SetVirtualCamera(CinemachineCamera vcam)
        {
            virtualCamera = vcam;
            _thirdPersonFollow = vcam?.GetComponent<CinemachineThirdPersonFollow>();
            
            if (virtualCamera != null && cameraTarget != null)
            {
                virtualCamera.Follow = cameraTarget;
                virtualCamera.LookAt = cameraTarget;
            }
        }
    }
}
