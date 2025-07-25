using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayPerfect.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _turnText;
        [SerializeField] Button _replayButton;
        
        [Header("Cells")]
        [SerializeField] CellButton[] _cellButtons; 
        
        [Header("Sprites")]
        [SerializeField] Sprite _xSprite;
        [SerializeField] Sprite _oSprite;

        Action<int, int> _onCellClickedCallback;
        Action _onReplayButtonClickEvent;
        
        void Start()
        {
            _replayButton.onClick.RemoveAllListeners();;
            _replayButton.onClick.AddListener(OnReplayButtonClicked);
            _replayButton.gameObject.SetActive(false);
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

            ResetCells();
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
    }
}
