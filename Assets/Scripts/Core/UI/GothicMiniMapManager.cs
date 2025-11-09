// GothicMiniMapManager: ミニマップ管理
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.UI
{
    /// <summary>
    /// ミニマップの表示とプレイヤー位置の更新
    /// </summary>
    public class GothicMiniMapManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Camera miniMapCamera;

        [Header("UI Elements")]
        [SerializeField] private RawImage miniMapImage;
        [SerializeField] private RectTransform playerIcon;

        [Header("Settings")]
        [SerializeField] private float mapZoom = 50f;
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private bool forceRenderEveryFrame = false; // デバッグ用

        private float updateTimer = 0f;
        private RenderTexture renderTexture;

        private void Start()
        {
            Debug.Log($"[GothicMiniMapManager] Start called. MiniMapImage: {miniMapImage != null}");
            
            // miniMapImageがNULLの場合、自動検索
            if (miniMapImage == null)
            {
                Debug.LogWarning("[GothicMiniMapManager] MiniMapImage is NULL! Searching...");
                
                // MapDisplayを探す
                Transform mapDisplay = transform.Find("MapDisplay");
                if (mapDisplay == null)
                {
                    Transform mapContainer = transform.Find("MapContainer");
                    if (mapContainer != null)
                    {
                        mapDisplay = mapContainer.Find("MapDisplay");
                    }
                }
                
                if (mapDisplay != null)
                {
                    miniMapImage = mapDisplay.GetComponent<RawImage>();
                    Debug.Log($"[GothicMiniMapManager] MiniMapImage found: {miniMapImage != null}");
                }
                else
                {
                    Debug.LogError("[GothicMiniMapManager] MapDisplay not found!");
                }
            }
            
            // プレイヤーを自動検索
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log($"[GothicMiniMapManager] Player found: {player.name}");
                }
                else
                {
                    Debug.LogWarning("[GothicMiniMapManager] Player not found! Make sure Player has 'Player' tag.");
                }
            }
            
            InitializeMiniMap();
            
            // RawImageにテクスチャが設定されているか確認
            if (miniMapImage != null && miniMapImage.texture == null && renderTexture != null)
            {
                Debug.LogWarning("[GothicMiniMapManager] RawImage.texture is NULL! Re-assigning RenderTexture...");
                miniMapImage.texture = renderTexture;
            }
            
            // 初期位置を設定
            if (playerTransform != null && miniMapCamera != null)
            {
                UpdateMiniMap();
                Debug.Log($"[GothicMiniMapManager] Initial camera position set: {miniMapCamera.transform.position}");
            }
        }

        private void InitializeMiniMap()
        {
            // ミニマップカメラを作成または再設定
            if (miniMapCamera == null)
            {
                GameObject cameraObj = new GameObject("MiniMapCamera");
                cameraObj.transform.SetParent(transform);
                miniMapCamera = cameraObj.AddComponent<Camera>();
                miniMapCamera.orthographic = true;
                miniMapCamera.orthographicSize = mapZoom;
                miniMapCamera.cullingMask = ~0; // すべてのレイヤーを表示
                miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
                miniMapCamera.backgroundColor = new Color(0.2f, 0.3f, 0.2f, 1f);
                miniMapCamera.depth = -10;
                miniMapCamera.enabled = true; // カメラを有効化
                miniMapCamera.allowHDR = false;
                miniMapCamera.allowMSAA = false;

                Debug.Log("[GothicMiniMapManager] MiniMap camera created");
            }
            else
            {
                // カメラが既に存在する場合も設定を確認
                miniMapCamera.enabled = true;
                miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
                miniMapCamera.backgroundColor = new Color(0.2f, 0.3f, 0.2f, 1f);
            }

            // RenderTextureを作成または再作成
            if (renderTexture == null || !renderTexture.IsCreated())
            {
                if (renderTexture != null)
                {
                    renderTexture.Release();
                }
                
                renderTexture = new RenderTexture(256, 256, 16);
                renderTexture.name = "MiniMapRenderTexture";
                renderTexture.filterMode = FilterMode.Bilinear;
                renderTexture.antiAliasing = 1;
                renderTexture.Create();
                
                Debug.Log($"[GothicMiniMapManager] RenderTexture created: {renderTexture.name} ({renderTexture.width}x{renderTexture.height})");
            }

            // カメラにRenderTextureを設定
            if (miniMapCamera.targetTexture != renderTexture)
            {
                miniMapCamera.targetTexture = renderTexture;
                Debug.Log("[GothicMiniMapManager] RenderTexture assigned to camera");
            }

            // RawImageにRenderTextureを設定
            if (miniMapImage != null)
            {
                if (miniMapImage.texture != renderTexture)
                {
                    miniMapImage.texture = renderTexture;
                    Debug.Log("[GothicMiniMapManager] RenderTexture assigned to RawImage");
                }
                
                miniMapImage.enabled = true; // RawImageを有効化
                miniMapImage.color = Color.white; // 色を白に設定
                miniMapImage.uvRect = new Rect(0, 0, 1, 1); // UV座標を設定
                miniMapImage.raycastTarget = false; // レイキャストを無効化
                
                Debug.Log($"[GothicMiniMapManager] RawImage configured: enabled={miniMapImage.enabled}, texture={miniMapImage.texture != null}");
                Debug.Log($"[GothicMiniMapManager] RawImage color: {miniMapImage.color}, uvRect: {miniMapImage.uvRect}");
            }
            else
            {
                Debug.LogError("[GothicMiniMapManager] MiniMapImage is NULL!");
            }

            Debug.Log($"[GothicMiniMapManager] Camera enabled: {miniMapCamera.enabled}, cullingMask: {miniMapCamera.cullingMask}");
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateMiniMap();
            }
            
            // デバッグ用: 毎フレーム強制レンダリング
            if (forceRenderEveryFrame && miniMapCamera != null)
            {
                miniMapCamera.Render();
            }
        }

        private void UpdateMiniMap()
        {
            if (playerTransform == null || miniMapCamera == null) return;

            // カメラをプレイヤーの上に配置
            Vector3 cameraPos = playerTransform.position;
            cameraPos.y += 50f; // プレイヤーの上50m
            miniMapCamera.transform.position = cameraPos;
            miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 真下を向く
            
            // デバッグログを削除（正常動作確認済み）
            // if (Time.frameCount < 10)
            // {
            //     Debug.Log($"[GothicMiniMapManager] Camera pos: {cameraPos}, Player pos: {playerTransform.position}");
            // }
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Zoom In")]
        private void TestZoomIn()
        {
            mapZoom = Mathf.Max(10f, mapZoom - 10f);
            if (miniMapCamera != null)
            {
                miniMapCamera.orthographicSize = mapZoom;
            }
        }

        [ContextMenu("Test: Zoom Out")]
        private void TestZoomOut()
        {
            mapZoom = Mathf.Min(100f, mapZoom + 10f);
            if (miniMapCamera != null)
            {
                miniMapCamera.orthographicSize = mapZoom;
            }
        }

        [ContextMenu("Debug: Check Status")]
        private void DebugCheckStatus()
        {
            Debug.Log("=== MiniMap Status ===");
            Debug.Log($"MiniMapImage: {miniMapImage != null}");
            Debug.Log($"MiniMapCamera: {miniMapCamera != null}");
            Debug.Log($"RenderTexture: {renderTexture != null}");
            Debug.Log($"PlayerTransform: {playerTransform != null}");
            
            if (miniMapImage != null)
            {
                Debug.Log($"RawImage.texture: {miniMapImage.texture != null}");
                Debug.Log($"RawImage.color: {miniMapImage.color}");
                Debug.Log($"RawImage.enabled: {miniMapImage.enabled}");
            }
            
            if (miniMapCamera != null)
            {
                Debug.Log($"Camera.targetTexture: {miniMapCamera.targetTexture != null}");
                Debug.Log($"Camera.enabled: {miniMapCamera.enabled}");
                Debug.Log($"Camera.position: {miniMapCamera.transform.position}");
            }
        }

        [ContextMenu("Debug: Force Reinitialize")]
        private void DebugForceReinitialize()
        {
            Debug.Log("[GothicMiniMapManager] Force reinitializing...");
            
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
            
            if (miniMapCamera != null)
            {
                Destroy(miniMapCamera.gameObject);
                miniMapCamera = null;
            }
            
            InitializeMiniMap();
            Debug.Log("[GothicMiniMapManager] Reinitialization complete!");
        }
#endif
        #endregion
    }
}
