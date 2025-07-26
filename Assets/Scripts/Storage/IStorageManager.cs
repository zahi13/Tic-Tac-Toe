namespace PlayPerfect.SaveSystem
{
    public interface IStorageManager<T>
    {
        void Save(string key, T data);
        T Load(string key);
        bool HasKey(string key);
        void Delete(string key);
    }
}

