// PlayerAnimationController: Animator連携
using UnityEngine;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤーアニメーション制御
    /// Phase 1実装: Animator連携、アニメーションパラメータ管理
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Animator animator;

        [Header("Animation Settings")]
        [SerializeField] private float speedSmoothTime = 0.1f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        // Animator Parameters (Hash化でパフォーマンス向上)
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int TurnSpeed = Animator.StringToHash("TurnSpeed");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int Death = Animator.StringToHash("Death");

        // スムージング用
        private float currentSpeed = 0f;
        private float speedVelocity = 0f;
        private float currentTurnSpeed = 0f;
        private float turnVelocity = 0f;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (animator == null)
            {
                Debug.LogError("[PlayerAnimationController] Animator component is missing!");
            }

            if (playerController == null)
            {
                Debug.LogWarning("[PlayerAnimationController] PlayerController reference is missing!");
            }
        }

        private void Update()
        {
            if (animator == null || playerController == null) return;

            UpdateMovementAnimation();
        }

        /// <summary>
        /// 移動アニメーションを更新
        /// </summary>
        private void UpdateMovementAnimation()
        {
            // 現在の速度を取得
            float targetSpeed = playerController.GetCurrentSpeed();
            bool isGrounded = playerController.IsGrounded();

            // スムージング
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

            // アニメーションパラメータを更新
            animator.SetFloat(Speed, currentSpeed);
            animator.SetBool(IsGrounded, isGrounded);
            animator.SetBool(IsMoving, currentSpeed > 0.1f);
        }

        /// <summary>
        /// ジャンプアニメーションをトリガー
        /// </summary>
        public void TriggerJump()
        {
            if (animator != null)
            {
                animator.SetTrigger(Jump);
                Debug.Log("[PlayerAnimationController] Jump animation triggered");
            }
        }

        /// <summary>
        /// ダッシュ状態を設定
        /// </summary>
        public void SetSprinting(bool isSprinting)
        {
            if (animator != null)
            {
                animator.SetBool(IsSprinting, isSprinting);
            }
        }

        /// <summary>
        /// 攻撃アニメーションをトリガー
        /// </summary>
        public void TriggerAttack(int attackIndex = 0)
        {
            if (animator != null)
            {
                animator.SetInteger("AttackIndex", attackIndex);
                animator.SetTrigger(Attack);
                Debug.Log($"[PlayerAnimationController] Attack animation triggered (Index: {attackIndex})");
            }
        }

        /// <summary>
        /// ヒットアニメーションをトリガー
        /// </summary>
        public void TriggerHit()
        {
            if (animator != null)
            {
                animator.SetTrigger(Hit);
                Debug.Log("[PlayerAnimationController] Hit animation triggered");
            }
        }

        /// <summary>
        /// 死亡アニメーションをトリガー
        /// </summary>
        public void TriggerDeath()
        {
            if (animator != null)
            {
                animator.SetTrigger(Death);
                Debug.Log("[PlayerAnimationController] Death animation triggered");
            }
        }

        /// <summary>
        /// 回転速度を設定
        /// </summary>
        public void SetTurnSpeed(float turnSpeed)
        {
            if (animator != null)
            {
                currentTurnSpeed = Mathf.SmoothDamp(currentTurnSpeed, turnSpeed, ref turnVelocity, turnSmoothTime);
                animator.SetFloat(TurnSpeed, currentTurnSpeed);
            }
        }

        /// <summary>
        /// アニメーションをリセット
        /// </summary>
        public void ResetAnimation()
        {
            if (animator != null)
            {
                animator.SetFloat(Speed, 0f);
                animator.SetBool(IsMoving, false);
                animator.SetBool(IsSprinting, false);
                currentSpeed = 0f;
                speedVelocity = 0f;
            }
        }

        /// <summary>
        /// 特定のアニメーションレイヤーの重みを設定
        /// </summary>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (animator != null && layerIndex < animator.layerCount)
            {
                animator.SetLayerWeight(layerIndex, weight);
            }
        }

        /// <summary>
        /// 現在のアニメーション状態を取得
        /// </summary>
        public AnimatorStateInfo GetCurrentStateInfo(int layerIndex = 0)
        {
            if (animator != null)
            {
                return animator.GetCurrentAnimatorStateInfo(layerIndex);
            }
            return default;
        }

        /// <summary>
        /// アニメーションが再生中かチェック
        /// </summary>
        public bool IsPlayingAnimation(string stateName, int layerIndex = 0)
        {
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                return stateInfo.IsName(stateName);
            }
            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Trigger Jump")]
        private void TestJump()
        {
            TriggerJump();
        }

        [ContextMenu("Test: Trigger Attack")]
        private void TestAttack()
        {
            TriggerAttack(0);
        }

        [ContextMenu("Test: Trigger Hit")]
        private void TestHit()
        {
            TriggerHit();
        }

        [ContextMenu("Test: Reset Animation")]
        private void TestReset()
        {
            ResetAnimation();
        }
#endif
    }
}
