using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Project.Editor
{
    /// <summary>
    /// Player Animator Controllerを作成するエディタスクリプト
    /// </summary>
    public class PlayerAnimatorCreator
    {
        [MenuItem("Tools/Create Player Animator Controller")]
        public static void CreatePlayerAnimatorController()
        {
            // Animator Controllerを作成
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
                "Assets/Animation/Player/PlayerAnimator.controller");
            
            // レイヤーを取得
            AnimatorControllerLayer baseLayer = controller.layers[0];
            
            // パラメータを追加
            // Speed（float）
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            
            // IsGrounded（bool）
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            
            // Jump（trigger）
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            
            // IsSprinting（bool）
            controller.AddParameter("IsSprinting", AnimatorControllerParameterType.Bool);
            
            // ステートマシンを設定
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            
            // Idleステート
            AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(-200, 0, 0));
            idleState.motion = GetDefaultClip("Idle");
            
            // Walkステート
            AnimatorState walkState = stateMachine.AddState("Walk", new Vector3(0, 0, 0));
            walkState.motion = GetDefaultClip("Walk");
            
            // Runステート
            AnimatorState runState = stateMachine.AddState("Run", new Vector3(200, 0, 0));
            runState.motion = GetDefaultClip("Run");
            
            // Jumpステート
            AnimatorState jumpState = stateMachine.AddState("Jump", new Vector3(0, -150, 0));
            jumpState.motion = GetDefaultClip("Jump");
            
            // デフォルトステートをIdleに設定
            stateMachine.defaultState = idleState;
            
            // トランジションを設定
            // Idle -> Walk
            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.duration = 0.1f;
            
            // Walk -> Idle
            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.duration = 0.1f;
            
            // Walk -> Run
            AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 5f, "Speed");
            walkToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsSprinting");
            walkToRun.duration = 0.1f;
            
            // Run -> Walk
            AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.Less, 5f, "Speed");
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsSprinting");
            runToWalk.duration = 0.1f;
            
            // Any State -> Jump
            AnimatorStateTransition anyToJump = stateMachine.AddAnyStateTransition(jumpState);
            anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            anyToJump.duration = 0.1f;
            anyToJump.canTransitionToSelf = false;
            
            // Jump -> Idle
            AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            jumpToIdle.duration = 0.1f;
            
            // AssetDatabaseを保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Project.Debug.Debug.Log("Player Animator Controller created at: Assets/Animation/Player/PlayerAnimator.controller");
        }
        
        private static AnimationClip GetDefaultClip(string clipName)
        {
            // デフォルトのアニメーションクリップを作成（1秒の空クリップ）
            AnimationClip clip = new AnimationClip();
            clip.name = clipName;
            
            // ループ設定
            if (clipName == "Idle" || clipName == "Walk" || clipName == "Run")
            {
                AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings
                {
                    loopTime = true,
                    loopBlend = true
                });
            }
            
            return clip;
        }
    }
}
