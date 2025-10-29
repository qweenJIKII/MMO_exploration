// 1分ごとのメトリクスCSV書き出し（fps/ram_mb等）
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Project.Core.Observability
{
    public class MetricsSampler : MonoBehaviour
    {
        [Tooltip("サンプリング間隔（秒）")] public int intervalSeconds = 60;
        private string _dir;
        private string _file;
        private float _accumDelta;
        private int _accumFrames;
        private float _timer;

        private void Start()
        {
            _dir = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
            _file = Path.Combine(_dir, "metrics.csv");
            if (!File.Exists(_file))
            {
                File.WriteAllText(_file, "ts,fps,ram_mb\n", Encoding.UTF8);
            }
        }

        private void Update()
        {
            _accumDelta += Time.unscaledDeltaTime;
            _accumFrames++;
            _timer += Time.unscaledDeltaTime;
            if (_timer >= intervalSeconds)
            {
                float fps = _accumFrames / Mathf.Max(_accumDelta, 1e-6f);
                float ramMb = (float)(GC.GetTotalMemory(false) / (1024.0 * 1024.0));
                var line = string.Format("{0:O},{1:F1},{2:F1}\n", DateTime.UtcNow, fps, ramMb);
                File.AppendAllText(_file, line, Encoding.UTF8);
                _timer = 0f; _accumDelta = 0f; _accumFrames = 0;
            }
        }
    }
}
