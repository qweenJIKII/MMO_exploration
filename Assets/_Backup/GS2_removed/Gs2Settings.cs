using UnityEngine;

namespace Project.Core.Online
{
    // GS2接続設定（ScriptableObject）。Resources に配置して参照します。
    [CreateAssetMenu(fileName = "Gs2Settings", menuName = "Project/GS2 Settings", order = 0)]
    public sealed class Gs2Settings : ScriptableObject
    {
        [Header("GS2 基本設定")]
        [Tooltip("GS2のNamespace名（例: MMO_My_Inventory）")]
        public string NamespaceName = "mmoexp-dev";

        [Tooltip("Inventoryのモデル名（例: main）")]
        public string InventoryModelName = "main";

        [Tooltip("Distributor の Namespace 名。空の場合は NamespaceName を使用（StampSheet 自動実行向け）")]
        public string DistributorNamespaceName = "";

        [Tooltip("リージョン（例: ap-northeast-1 など）。未使用時は空で自動設定")]
        public string Region = "";

        [Tooltip("RESTエンドポイントを手動指定したい場合のみ（通常は空）")]
        public string EndpointOverride = "";

        [Header("クライアント側はAPIキーを保持しません")] 
        [TextArea]
        public string Notes = "Client does not store admin credentials. Use Identifier sign-in and access tokens.";
    }
}
