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
        [SerializeField] RowTypesContainer _rowTypesContainer;
        
        [Header("Cells")]
        [SerializeField] CellButton[] _cellButtons; 
        
        Sprite _xSprite;
        Sprite _oSprite;
        public bool IsLoadingAssetsCompleted { get; private set; }

        GameManager _gameManager;
        
        public async UniTask LoadSpritesAsync()
        {
            IsLoadingAssetsCompleted = false;
            var loadAssets = Addressables.LoadAssetAsync<Sprite[]>(AssetsNames.SPRITES_ASSETS_PATH);
            var sprites = await loadAssets.ToUniTask();

            if (sprites == null || !sprites.Any())
            {
                Debug.LogError("Missing sprites.");
                return;
            }

            Sprite rowSprite = null, diagonalRowSprite = null;
            foreach (var sprite in sprites)
            {
                switch (sprite.name)
                {
                    case AssetsNames.X_SPRITE_ASSET_NAME:
                        _xSprite = sprite;
                        break;
                    case AssetsNames.O_SPRITE_ASSET_NAME:
                        _oSprite = sprite;
                        break;
                    case AssetsNames.GRID_SPRITE_ASSET_NAME:
                        _gridBackgroundImage.sprite = sprite;
                        break;
                    case AssetsNames.ROW_SPRITE_ASSET_NAME:
                        rowSprite = sprite;
                        break;
                    case AssetsNames.DIAGONAL_ROW_SPRITE_ASSET_NAME:
                        diagonalRowSprite = sprite;
                        break;
                    
                    default:
                        Debug.LogError($"No match for sprite named {sprite.name}.");
                        break;
                }
            }

            _rowTypesContainer.SetSprites(rowSprite, diagonalRowSprite);
            IsLoadingAssetsCompleted = true;
        }

        public void Initialize(GameManager gameManager, Action onReplayButtonClickCallback, Action<int, int> onCellClickedCallback)
        {
            _gameManager = gameManager;

            for (var index = 0; index < _cellButtons.Length; index++)
            {
                var row = index / 3;     // 0, 1, 2
                var column = index % 3;  // 0, 1, 2
                var cell = _cellButtons[index];
                cell.Initialize(row, column, cellButton =>
                {
                    var (row, column) = cellButton.GetCoordinates();
                    onCellClickedCallback?.Invoke(row, column);
                });
                cell.name = $"CellButton_({row},{column})";
            }
            
            _replayButton.onClick.RemoveAllListeners();;
            _replayButton.onClick.AddListener(() =>
            {
                _replayButton.gameObject.SetActive(false);
                onReplayButtonClickCallback?.Invoke();
            });
            _replayButton.gameObject.SetActive(false);
        }
        
        public void UpdateCellVisual(int row, int column, string symbol)
        {
            foreach (var cell in _cellButtons)
            {
                var (rowIndex, columnIndex) = cell.GetCoordinates();
                if (row != rowIndex || column != columnIndex) continue;
                if (symbol == null)
                {
                    cell.UpdateSprite(null);
                    cell.ToggleInteraction(true);
                }
                else
                {
                    cell.UpdateSprite(symbol == AssetsNames.X_SPRITE_ASSET_NAME ? _xSprite : _oSprite);
                    cell.ToggleInteraction(false);
                }
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
            
            _rowTypesContainer.Reset();
        }

        public void UpdateScore(int score, int bestScore)
        {
            _scoreText.text = $"Score: {score} / Best: {bestScore}";
        }

        public void UpdateTurnText(bool isPlayerTurn)
        {
            _turnText.text = isPlayerTurn ? "Your Turn" : "Computer's Turn";
        }

        public void OnGameOverHandler()
        {
            _replayButton.gameObject.SetActive(true);

            var finalScore = _gameManager.GetFinalScore();
            var bestScore = _gameManager.GetBestScore();
            bestScore = Math.Max(finalScore, bestScore);
            UpdateScore(finalScore, bestScore);
            ToggleCellsInteraction(false);
            UpdateGameResultText(_gameManager.Result);
            _rowTypesContainer.ActivateRowType(_gameManager.RowResult);
            _turnText.text = string.Empty;
        }

        void ToggleCellsInteraction(bool state)
        {
            foreach (var cell in _cellButtons)
                cell.ToggleInteraction(state);
        }
        
        public void ToggleEmptyCellsInteraction(bool enable, int[,] board)
        {
            foreach (var cell in _cellButtons)
            {
                var (row, column) = cell.GetCoordinates();
                cell.ToggleInteraction(enable && board[row, column] == 0);
            }
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

        public void SetBoard(int[,] board)
        {
            for (var row = 0; row < board.GetLength(0); row++)
            for (var column = 0; column < board.GetLength(1); column++)
            {
                var symbol = string.Empty;
                var symbolIndex = board[row, column];
                symbol = symbolIndex switch
                {
                    0 => null,
                    1 => AssetsNames.X_SPRITE_ASSET_NAME,
                    2 => AssetsNames.O_SPRITE_ASSET_NAME,
                    _ => symbol
                };
                UpdateCellVisual(row, column, symbol);
            }
        }
    }
    
    public class AssetsNames
    {
        public const string SPRITES_ASSETS_PATH = "GraphicAssets"; 

        public const string X_SPRITE_ASSET_NAME = "X"; 
        public const string O_SPRITE_ASSET_NAME = "O"; 
        public const string GRID_SPRITE_ASSET_NAME = "Grid"; 
        public const string ROW_SPRITE_ASSET_NAME = "Row"; 
        public const string DIAGONAL_ROW_SPRITE_ASSET_NAME = "DiagonalRow"; 
    }
}
