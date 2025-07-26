namespace PlayPerfect.StorageSystem
{
    public class StorageManager<T>
    {
        readonly IStorageManager<T> _storageManager;

        public StorageManager(IStorageManager<T> storageManager)
        {
            _storageManager = storageManager;
        }

        public void Save(string key, T data) => _storageManager.Save(key, data);
        public T Load(string key) => _storageManager.Load(key);
        public bool HasKey(string key) => _storageManager.HasKey(key);
        public void Delete(string key) => _storageManager.Delete(key);
    }
}

