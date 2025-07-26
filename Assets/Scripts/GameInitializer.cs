using PlayPerfect.Core;
using PlayPerfect.SaveSystem;
using PlayPerfect.UI;
using UnityEngine;
using UnityEngine.EventSystems;

// Acts as a single point of entry for the game's initialization
// and initializes the game with the specified dependencies
public class GameInitializer : MonoBehaviour
{
    [Header("General References")]
    [SerializeField] EventSystem _eventSystem;
    [SerializeField] Camera _mainCamera;
    [SerializeField] UIManager _uiManager;
    [SerializeField] ApplicationEventsHandler _applicationEventsHandler;

    async void Start()
    {
        _eventSystem = Instantiate(_eventSystem);
        _mainCamera = Instantiate(_mainCamera);
        _uiManager = Instantiate(_uiManager);
        _applicationEventsHandler = Instantiate(_applicationEventsHandler);

        await _uiManager.LoadSpritesAsync();
        if (!_uiManager.IsLoadingAssetsCompleted)
        {
            Debug.LogError("Sprites failed to load. Cannot start game.");
            return;
        }

        var storageManager = CreateStorageManager();
        new GameManager(_uiManager, storageManager).Initialize();

        Destroy(gameObject);
    }

    StorageManager<GameManager.GameState> CreateStorageManager()
    {
        StorageManager<GameManager.GameState> storageManager;
#if UNITY_EDITOR
        storageManager = new StorageManager<GameManager.GameState>(new EditorStorageManager<GameManager.GameState>());
#else
        storageManager = new StorageManager<GameManager.GameState>(new PlayerPrefsStorageManager<GameManager.GameState>());
#endif
        return storageManager;
    }
}
