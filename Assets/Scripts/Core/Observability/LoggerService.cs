// ロガーサービス: 構造化ログの統合管理
using System;
using UnityEngine;

namespace Project.Core.Observability
{
    /// <summary>
    /// 構造化ログを管理するサービス（JsonlLoggerのラッパー）
    /// </summary>
    public static class LoggerService
    {
        private static string _sessionId;

        static LoggerService()
        {
            // セッションIDを生成（アプリケーション起動時に一度だけ）
            _sessionId = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// ログレベル
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// 構造化ログイベント
        /// </summary>
        [Serializable]
        public class LogEvent
        {
            public string timestamp;
            public string level;
            public string category;
            public string message;
            public string playerId;
            public string sessionId;
            public string stackTrace;

            public LogEvent(LogLevel level, string category, string message, string stackTrace = null)
            {
                this.timestamp = DateTime.UtcNow.ToString("O");
                this.level = level.ToString();
                this.category = category;
                this.message = message;
                this.playerId = GetPlayerId();
                this.sessionId = GetSessionId();
                this.stackTrace = stackTrace;
            }

            private static string GetPlayerId()
            {
                try
                {
                    return Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn
                        ? Unity.Services.Authentication.AuthenticationService.Instance.PlayerId
                        : "anonymous";
                }
                catch
                {
                    return "unknown";
                }
            }

            private static string GetSessionId()
            {
                return LoggerService._sessionId;
            }
        }

        /// <summary>
        /// カスタムイベント（ゲームプレイイベント等）
        /// </summary>
        [Serializable]
        public class CustomEvent
        {
            public string timestamp;
            public string eventType;
            public string playerId;
            public string sessionId;
            public string data;

            public CustomEvent(string eventType, object data)
            {
                this.timestamp = DateTime.UtcNow.ToString("O");
                this.eventType = eventType;
                this.playerId = GetPlayerId();
                this.sessionId = GetSessionId();
                this.data = JsonUtility.ToJson(data);
            }

            private static string GetPlayerId()
            {
                try
                {
                    return Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn
                        ? Unity.Services.Authentication.AuthenticationService.Instance.PlayerId
                        : "anonymous";
                }
                catch
                {
                    return "unknown";
                }
            }

            private static string GetSessionId()
            {
                return LoggerService._sessionId;
            }
        }

        // ========== Public API ==========

        /// <summary>
        /// デバッグログを記録
        /// </summary>
        public static void Debug(string category, string message)
        {
            Log(LogLevel.Debug, category, message);
        }

        /// <summary>
        /// 情報ログを記録
        /// </summary>
        public static void Info(string category, string message)
        {
            Log(LogLevel.Info, category, message);
        }

        /// <summary>
        /// 警告ログを記録
        /// </summary>
        public static void Warning(string category, string message)
        {
            Log(LogLevel.Warning, category, message);
        }

        /// <summary>
        /// エラーログを記録
        /// </summary>
        public static void Error(string category, string message, string stackTrace = null)
        {
            Log(LogLevel.Error, category, message, stackTrace);
        }

        /// <summary>
        /// 重大エラーログを記録
        /// </summary>
        public static void Critical(string category, string message, string stackTrace = null)
        {
            Log(LogLevel.Critical, category, message, stackTrace);
        }

        /// <summary>
        /// カスタムイベントを記録
        /// </summary>
        public static void LogCustomEvent(string eventType, object data)
        {
            var evt = new CustomEvent(eventType, data);
            JsonlLogger.LogEvent(evt);
        }

        /// <summary>
        /// 例外を記録
        /// </summary>
        public static void LogException(string category, Exception exception)
        {
            Error(category, exception.Message, exception.StackTrace);
        }

        // ========== Private Methods ==========

        private static void Log(LogLevel level, string category, string message, string stackTrace = null)
        {
            var evt = new LogEvent(level, category, message, stackTrace);
            JsonlLogger.LogEvent(evt);

            // Unityコンソールにも出力
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log($"[{category}] {message}");
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning($"[{category}] {message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError($"[{category}] {message}");
                    break;
            }
        }
    }
}
