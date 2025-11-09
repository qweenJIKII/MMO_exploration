// SimplePlayerMovement: テスト用の簡易プレイヤー移動
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.Player
{
    /// <summary>
    /// テスト用の簡易プレイヤー移動（New Input System使用）
    /// WASDキーで移動、Spaceキーでジャンプ
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimplePlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float rotationSpeed = 10.0f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5.0f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private bool _isGrounded;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            Debug.Log("[SimplePlayerMovement] Initialized");
        }

        private void Update()
        {
            // 地面判定
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            // 入力取得（New Input System）
            Vector2 moveInput = Vector2.zero;
            bool isSprinting = false;
            bool jumpPressed = false;

            if (Keyboard.current != null)
            {
                // WASD移動
                if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
                if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
                if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
                if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

                // Shift でダッシュ
                isSprinting = Keyboard.current.leftShiftKey.isPressed;

                // Space でジャンプ
                jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }

            // 移動処理
            if (moveInput.sqrMagnitude > 0.01f)
            {
                // カメラの向きを基準に移動方向を計算
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;

                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

                // 移動速度を決定
                float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

                // 水平移動を適用
                Vector3 horizontalVelocity = moveDirection * currentSpeed;
                _velocity.x = horizontalVelocity.x;
                _velocity.z = horizontalVelocity.z;

                // キャラクターを移動方向に回転
                if (moveDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                // 入力がない場合は水平速度をゼロに
                _velocity.x = 0;
                _velocity.z = 0;
            }

            // ジャンプ処理
            if (jumpPressed && _isGrounded)
            {
                _velocity.y = jumpForce;
                Debug.Log("[SimplePlayerMovement] Jump!");
            }

            // 重力を適用
            _velocity.y += gravity * Time.deltaTime;

            // CharacterControllerで移動
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            // 地面判定の可視化
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}
