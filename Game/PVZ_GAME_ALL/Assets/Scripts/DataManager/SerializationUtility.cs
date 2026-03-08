using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace DataManager
{
    public static class SerializationUtility
    {
        public static string Serialize<T>(T data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SerializationUtility] Cannot serialize null data");
                return null;
            }

            try
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerializationUtility] Serialization failed: {e.Message}");
                return null;
            }
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[SerializationUtility] Cannot deserialize null or empty JSON");
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerializationUtility] Deserialization failed: {e.Message}");
                return default;
            }
        }

        public static bool SaveToFile<T>(T data, string filePath)
        {
            if (data == null)
            {
                Debug.LogWarning("[SerializationUtility] Cannot save null data");
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerializationUtility] Failed to save file: {e.Message}");
                return false;
            }
        }

        public static T LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SerializationUtility] File not found: {filePath}");
                return default;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerializationUtility] Failed to load file: {e.Message}");
                return default;
            }
        }

        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
