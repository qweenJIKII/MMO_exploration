// DebugDrawManager テスト用スクリプト
using UnityEngine;
using Project.Dev;

namespace Project.Dev
{
    /// <summary>
    /// DebugDrawManagerの動作確認用テストスクリプト
    /// </summary>
    public class DebugDrawTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTest = false; // デフォルトは無効（Inspectorで有効化）
        [SerializeField] private float drawDuration = 0.1f;

        private void Update()
        {
            if (!enableTest) return;

            // 原点から上方向に赤いライン
            DebugDrawManager.DrawLine(Vector3.zero, Vector3.up * 2, Color.red, drawDuration);

            // 右側に緑のワイヤーキューブ
            DebugDrawManager.DrawWireCube(Vector3.right * 2, Vector3.one, Color.green, drawDuration);

            // 左側に青のワイヤースフィア
            DebugDrawManager.DrawWireSphere(Vector3.left * 2, 0.5f, Color.blue, drawDuration);

            // 前方に黄色のレイ
            DebugDrawManager.DrawRay(Vector3.zero, Vector3.forward * 3, Color.yellow, drawDuration);

            // 回転するキューブ（マゼンタ）
            Vector3 rotatingPos = new Vector3(
                Mathf.Sin(Time.time) * 2,
                1,
                Mathf.Cos(Time.time) * 2
            );
            DebugDrawManager.DrawCube(rotatingPos, Vector3.one * 0.5f, Color.magenta, drawDuration);

            // 回転するスフィア（シアン）
            Vector3 rotatingPos2 = new Vector3(
                Mathf.Cos(Time.time * 2) * 2,
                2,
                Mathf.Sin(Time.time * 2) * 2
            );
            DebugDrawManager.DrawSphere(rotatingPos2, 0.3f, Color.cyan, drawDuration);
        }

        private void OnDrawGizmos()
        {
            // Scene ビューでも確認できるように参照点を表示
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
        }
    }
}
