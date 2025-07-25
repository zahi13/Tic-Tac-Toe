using PlayPerfect.Core;
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

    async void Start()
    {
        _eventSystem = Instantiate(_eventSystem);
        _mainCamera = Instantiate(_mainCamera);
        _uiManager = Instantiate(_uiManager);

        await _uiManager.LoadSpritesAsync();
        if (!_uiManager.IsLoadingAssetsCompleted)
        {
            Debug.LogError("Sprites failed to load. Cannot start game.");
            return;
        }
        
        new GameManager(_uiManager).Initialize();

        Destroy(gameObject);
    }
}
