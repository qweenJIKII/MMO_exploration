// JSON Lines ロガー: 1イベント=1行JSONで追記、日次ローテーション
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Project.Core.Observability
{
    public static class JsonlLogger
    {
        private static readonly object _lock = new object();
        private static string _dir;
        private static DateTime _currentDay;
        private static string _filePath;

        static JsonlLogger()
        {
            _dir = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
            _currentDay = DateTime.UtcNow.Date;
            _filePath = Path.Combine(_dir, $"events_{_currentDay:yyyyMMdd}.jsonl");
        }

        private static void RotateIfNeeded()
        {
            var today = DateTime.UtcNow.Date;
            if (today != _currentDay)
            {
                _currentDay = today;
                _filePath = Path.Combine(_dir, $"events_{_currentDay:yyyyMMdd}.jsonl");
            }
        }

        public static void LogEvent(object evt)
        {
            try
            {
                RotateIfNeeded();
                var line = JsonUtility.ToJson(evt);
                lock (_lock)
                {
                    File.AppendAllText(_filePath, line + "\n", Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonlLogger] Write failed: {ex}");
            }
        }
    }
}
