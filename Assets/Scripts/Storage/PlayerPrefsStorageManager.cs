using Newtonsoft.Json;
using UnityEngine;

namespace PlayPerfect.StorageSystem
{
    public class PlayerPrefsStorageManager<T> : IStorageManager<T>
    {
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
            
            var json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public T Load(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return default;
            }
            
            if (!PlayerPrefs.HasKey(key)) return default;
            var json = PlayerPrefs.GetString(key);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool HasKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
                return PlayerPrefs.HasKey(key);
            Debug.LogError($"Missing key.");
            return false;
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Missing key.");
                return;
            }
            
            PlayerPrefs.DeleteKey(key);
        }
    }
}
