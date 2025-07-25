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

    void Start()
    {
        _eventSystem = Instantiate(_eventSystem);
        _mainCamera = Instantiate(_mainCamera);
        _uiManager = Instantiate(_uiManager);

        new GameManager(_uiManager).Initialize();

        Destroy(gameObject);
    }
}
