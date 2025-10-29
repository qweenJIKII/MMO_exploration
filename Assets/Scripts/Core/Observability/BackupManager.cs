// バックアップ管理: 3世代ローテーション
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Project.Core.Observability
{
    /// <summary>
    /// ファイルのバックアップを管理し、3世代ローテーションを実施
    /// </summary>
    public static class BackupManager
    {
        private const int MaxBackupGenerations = 3;

        /// <summary>
        /// 指定ファイルのバックアップを作成（3世代ローテーション）
        /// </summary>
        /// <param name="filePath">バックアップ対象ファイルのパス</param>
        /// <returns>バックアップファイルのパス（失敗時はnull）</returns>
        public static string CreateBackup(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogWarning($"[BackupManager] File not found: {filePath}");
                return null;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string backupPath = Path.Combine(directory, $"{fileName}.bak-{timestamp}{extension}");

                // バックアップファイルを作成
                File.Copy(filePath, backupPath, true);
                Debug.Log($"[BackupManager] Backup created: {backupPath}");

                // 古いバックアップを削除（3世代を超えた分）
                CleanupOldBackups(directory, fileName, extension);

                return backupPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BackupManager] Backup failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 古いバックアップファイルを削除（3世代を超えた分）
        /// </summary>
        private static void CleanupOldBackups(string directory, string fileName, string extension)
        {
            try
            {
                string searchPattern = $"{fileName}.bak-*{extension}";
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                FileInfo[] backupFiles = dirInfo.GetFiles(searchPattern)
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToArray();

                // 3世代を超えた古いバックアップを削除
                for (int i = MaxBackupGenerations; i < backupFiles.Length; i++)
                {
                    backupFiles[i].Delete();
                    Debug.Log($"[BackupManager] Old backup deleted: {backupFiles[i].Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BackupManager] Cleanup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 最新のバックアップファイルを取得
        /// </summary>
        /// <param name="filePath">元ファイルのパス</param>
        /// <returns>最新のバックアップファイルのパス（存在しない場合はnull）</returns>
        public static string GetLatestBackup(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string searchPattern = $"{fileName}.bak-*{extension}";

                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                FileInfo latestBackup = dirInfo.GetFiles(searchPattern)
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .FirstOrDefault();

                return latestBackup?.FullName;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BackupManager] Failed to get latest backup: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// バックアップからファイルを復元
        /// </summary>
        /// <param name="backupPath">バックアップファイルのパス</param>
        /// <param name="targetPath">復元先のパス</param>
        /// <returns>成功した場合true</returns>
        public static bool RestoreFromBackup(string backupPath, string targetPath)
        {
            if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
            {
                Debug.LogWarning($"[BackupManager] Backup file not found: {backupPath}");
                return false;
            }

            try
            {
                File.Copy(backupPath, targetPath, true);
                Debug.Log($"[BackupManager] Restored from backup: {backupPath} -> {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BackupManager] Restore failed: {ex.Message}");
                return false;
            }
        }
    }
}
