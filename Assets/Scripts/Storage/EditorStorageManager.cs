using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PlayPerfect.StorageSystem
{
    public class EditorStorageManager<T> : IStorageManager<T>
    {
        readonly string _saveDirectory;

        public EditorStorageManager()
        {
            _saveDirectory = Path.Combine(Application.dataPath, "Saves");
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        public void Save(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return;
            }
            
            if (data == null)
            {
                Debug.LogError($"{nameof(data)} is null.");
                return;
            }
            
            var path = Path.Combine(_saveDirectory, $"{key}.txt");
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }

        public T Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return default;
            }
            
            var path = Path.Combine(_saveDirectory, $"{key}.txt");
            if (!File.Exists(path))
            {
                Debug.LogError($"Path does not exist -> {path}");
                return default;
            }
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return default;
            }

            var path = Path.Combine(_saveDirectory, $"{key}.txt");
            return File.Exists(path);
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return;
            }

            var path = Path.Combine(_saveDirectory, $"{key}.txt");
            if (File.Exists(path)) 
                File.Delete(path);
        }
    }
}

