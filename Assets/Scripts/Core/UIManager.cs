using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlayPerfect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace PlayPerfect.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _turnText;
        [SerializeField] TMP_Text _resultText;
        [SerializeField] Button _replayButton;
        [SerializeField] Image _gridBackgroundImage;
        
        [Header("Cells")]
        [SerializeField] CellButton[] _cellButtons; 
        
        Action<int, int> _onCellClickedCallback;
        Action _onReplayButtonClickEvent;
        
        const string SPRITES_ASSETS_PATH = "GraphicAssets"; 
        Sprite _xSprite;
        Sprite _oSprite;
        public bool IsLoadingAssetsCompleted { get; private set; }

        void Start()
        {
            _replayButton.onClick.RemoveAllListeners();;
            _replayButton.onClick.AddListener(OnReplayButtonClicked);
            _replayButton.gameObject.SetActive(false);
        }
        
        public async UniTask LoadSpritesAsync()
        {
            IsLoadingAssetsCompleted = false;
            var loadAssets = Addressables.LoadAssetAsync<Sprite[]>(SPRITES_ASSETS_PATH);
            var sprites = await loadAssets.ToUniTask();

            if (sprites == null || !sprites.Any())
            {
                Debug.LogError("Missing sprites.");
                return;
            }
            
            foreach (var sprite in sprites)
            {
                switch (sprite.name)
                {
                    case "X":
                        _xSprite = sprite;
                        break;
                    case "O":
                        _oSprite = sprite;
                        break;
                    case "Grid":
                        _gridBackgroundImage.sprite = sprite;
                        break;
                }
            }

            if (_xSprite == null || _oSprite == null)
                Debug.LogError("Failed to locate X or O sprites in the sprite sheet.");
            else
                IsLoadingAssetsCompleted = true;
        }

        public void Initialize(Action onReplayButtonClickCallback, Action<int, int> onCellClickedCallback)
        {
            _onReplayButtonClickEvent = onReplayButtonClickCallback;
            _onCellClickedCallback = onCellClickedCallback;

            for (var index = 0; index < _cellButtons.Length; index++)
            {
                var row = index / 3;     // 0, 1, 2
                var column = index % 3;  // 0, 1, 2
                var cell = _cellButtons[index];
                cell.Initialize(row, column, HandleCellClicked);
                cell.name = $"CellButton_({row},{column})";
            }
        }
        
        void HandleCellClicked(CellButton cellButton)
        {
            var coordinates = cellButton.GetCoordinates();
            _onCellClickedCallback?.Invoke(coordinates.Item1, coordinates.Item2);
        }

        public void UpdateCellVisual(int row, int column, string symbol)
        {
            foreach (var cell in _cellButtons)
            {
                var coordinates = cell.GetCoordinates();
                if (coordinates.Item1 != row || coordinates.Item2 != column) continue;
                cell.UpdateSprite(symbol == "X" ? _xSprite : _oSprite);
                cell.ToggleInteraction(false);
                break;
            }
        }

        public void ResetCells()
        {
            foreach (var cell in _cellButtons)
            {
                cell.UpdateSprite(null);
                cell.ToggleInteraction(true);
            }
        }

        public void UpdateScore(int score, int bestScore)
        {
            _scoreText.text = $"Score: {score} / Best: {bestScore}";
        }

        public void UpdateTurnText(bool isPlayerTurn)
        {
            _turnText.text = isPlayerTurn ? "Your Turn" : "Computer's Turn";
        }
        
        public void ShowGameOver()
        {
            _replayButton.gameObject.SetActive(true);
        }

        void OnReplayButtonClicked()
        {
            _replayButton.gameObject.SetActive(false);
            _onReplayButtonClickEvent.Invoke();
        }

        public void ToggleCellsInteraction(bool state)
        {
            foreach (var cell in _cellButtons)
                cell.ToggleInteraction(state);
        }

        public void UpdateGameResultText(GameManager.GameResult result)
        {
            _resultText.text = result switch
            {
                GameManager.GameResult.None => string.Empty,
                GameManager.GameResult.Win => "You Won!",
                GameManager.GameResult.Lose => "You Lost!",
                GameManager.GameResult.Tie => "It's a Tie!",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
    }
}
