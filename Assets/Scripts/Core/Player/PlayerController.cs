// プレイヤーコントローラー: CharacterControllerを使用した移動・ジャンプ制御
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤーキャラクターの移動・ジャンプを制御
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float rotationSpeed = 10.0f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5.0f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer = -1; // すべてのレイヤー

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private bool _isGrounded;
        private Vector2 _moveInput;
        private bool _isSprinting;

        // Animator Parameters
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void OnEnable()
        {
            // Input Actionsを有効化
            if (moveAction != null)
                moveAction.action.Enable();
            if (jumpAction != null)
            {
                jumpAction.action.Enable();
                jumpAction.action.performed += OnJump;
            }
            if (sprintAction != null)
                sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            // Input Actionsを無効化
            if (moveAction != null)
                moveAction.action.Disable();
            if (jumpAction != null)
            {
                jumpAction.action.performed -= OnJump;
                jumpAction.action.Disable();
            }
            if (sprintAction != null)
                sprintAction.action.Disable();
        }

        private void Update()
        {
            // 入力を取得
            ReadInput();

            // 地面判定
            CheckGround();

            // 移動処理
            HandleMovement();

            // 重力とジャンプ
            HandleGravity();

            // CharacterControllerで移動
            _characterController.Move(_velocity * Time.deltaTime);

            // アニメーション更新
            UpdateAnimation();

            // プレイヤーデータの位置を更新（定期的に）
            if (Time.frameCount % 300 == 0) // 5秒ごと（60fps想定）
            {
                PlayerDataManager.Instance?.UpdatePosition(transform.position, transform.rotation);
            }
        }

        private void ReadInput()
        {
            // 移動入力
            if (moveAction != null)
            {
                _moveInput = moveAction.action.ReadValue<Vector2>();
            }

            // ダッシュ入力
            if (sprintAction != null)
            {
                _isSprinting = sprintAction.action.IsPressed();
            }
        }

        private void CheckGround()
        {
            // 地面判定（CharacterControllerの下部から少し下にRaycast）
            Vector3 spherePosition = transform.position - new Vector3(0, _characterController.height / 2, 0);
            _isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundLayer);

            // 地面に接地したら垂直速度をリセット
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // 小さな負の値で地面に押し付ける
            }
        }

        private void HandleMovement()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                return; // 入力がない場合は何もしない
            }

            // カメラの向きを基準に移動方向を計算
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            // Y軸成分を除去して正規化
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // 移動方向を計算
            Vector3 moveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;

            // 移動速度を決定
            float currentSpeed = _isSprinting ? sprintSpeed : moveSpeed;

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

        private void HandleGravity()
        {
            // 重力を適用
            _velocity.y += gravity * Time.deltaTime;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            // 地面にいる場合のみジャンプ
            if (_isGrounded)
            {
                _velocity.y = jumpForce;
                
                // アニメーショントリガー
                if (animator != null)
                {
                    animator.SetTrigger(Jump);
                }

                Debug.Log("[PlayerController] Jump!");
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            // 移動速度を計算（水平方向のみ）
            float horizontalSpeed = new Vector3(_velocity.x, 0, _velocity.z).magnitude;
            
            // アニメーションパラメータを更新
            animator.SetFloat(Speed, horizontalSpeed);
            animator.SetBool(IsGroundedParam, _isGrounded);
            animator.SetBool(IsSprinting, _isSprinting && horizontalSpeed > 0.1f);
        }

        private void OnDrawGizmosSelected()
        {
            // 地面判定の可視化
            if (_characterController != null)
            {
                Vector3 spherePosition = transform.position - new Vector3(0, _characterController.height / 2, 0);
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);
            }
        }

        /// <summary>
        /// プレイヤーを指定位置にテレポート
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _characterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            _velocity = Vector3.zero;
            _characterController.enabled = true;
            
            Debug.Log($"[PlayerController] Teleported to {position}");
        }

        /// <summary>
        /// 現在の移動速度を取得
        /// </summary>
        public float GetCurrentSpeed()
        {
            return new Vector3(_velocity.x, 0, _velocity.z).magnitude;
        }

        /// <summary>
        /// 地面にいるかどうかを取得
        /// </summary>
        public bool IsGrounded()
        {
            return _isGrounded;
        }
    }
}
