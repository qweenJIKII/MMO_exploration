// デバッグ描画管理: Gizmo描画の統合管理
using System.Collections.Generic;
using UnityEngine;

namespace Project.Dev
{
    /// <summary>
    /// デバッグ描画を管理するシングルトン
    /// </summary>
    public class DebugDrawManager : MonoBehaviour
    {
        private static DebugDrawManager _instance;
        public static DebugDrawManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DebugDrawManager");
                    _instance = go.AddComponent<DebugDrawManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private List<DebugDrawCommand> _drawCommands = new List<DebugDrawCommand>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // 期限切れのコマンドを削除
            _drawCommands.RemoveAll(cmd => Time.time > cmd.EndTime);
        }

        private void OnDrawGizmos()
        {
            foreach (var cmd in _drawCommands)
            {
                Gizmos.color = cmd.Color;
                switch (cmd.Type)
                {
                    case DrawType.Line:
                        Gizmos.DrawLine(cmd.Start, cmd.End);
                        break;
                    case DrawType.WireCube:
                        Gizmos.DrawWireCube(cmd.Center, cmd.Size);
                        break;
                    case DrawType.WireSphere:
                        Gizmos.DrawWireSphere(cmd.Center, cmd.Radius);
                        break;
                    case DrawType.Cube:
                        Gizmos.DrawCube(cmd.Center, cmd.Size);
                        break;
                    case DrawType.Sphere:
                        Gizmos.DrawSphere(cmd.Center, cmd.Radius);
                        break;
                    case DrawType.Ray:
                        Gizmos.DrawRay(cmd.Start, cmd.Direction);
                        break;
                }
            }
        }

        // ========== Public API ==========

        /// <summary>
        /// ラインを描画
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.Line,
                Start = start,
                End = end,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// ワイヤーキューブを描画
        /// </summary>
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.WireCube,
                Center = center,
                Size = size,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// ワイヤースフィアを描画
        /// </summary>
        public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.WireSphere,
                Center = center,
                Radius = radius,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// キューブを描画（塗りつぶし）
        /// </summary>
        public static void DrawCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.Cube,
                Center = center,
                Size = size,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// スフィアを描画（塗りつぶし）
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.Sphere,
                Center = center,
                Radius = radius,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// レイを描画
        /// </summary>
        public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration = 0f)
        {
            Instance._drawCommands.Add(new DebugDrawCommand
            {
                Type = DrawType.Ray,
                Start = start,
                Direction = direction,
                Color = color,
                EndTime = Time.time + duration
            });
        }

        /// <summary>
        /// すべての描画コマンドをクリア
        /// </summary>
        public static void Clear()
        {
            Instance._drawCommands.Clear();
        }

        // ========== Internal Types ==========

        private enum DrawType
        {
            Line,
            WireCube,
            WireSphere,
            Cube,
            Sphere,
            Ray
        }

        private struct DebugDrawCommand
        {
            public DrawType Type;
            public Vector3 Start;
            public Vector3 End;
            public Vector3 Center;
            public Vector3 Size;
            public float Radius;
            public Vector3 Direction;
            public Color Color;
            public float EndTime;
        }
    }
}
