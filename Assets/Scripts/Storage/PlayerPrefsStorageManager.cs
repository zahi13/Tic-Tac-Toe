using Newtonsoft.Json;
using UnityEngine;

namespace PlayPerfect.SaveSystem
{
    public class PlayerPrefsStorageManager<T> : IStorageManager<T>
    {
        public void Save(string key, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public T Load(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return default;
            var json = PlayerPrefs.GetString(key);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}
