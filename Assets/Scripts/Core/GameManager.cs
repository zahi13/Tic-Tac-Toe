using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using PlayPerfect.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayPerfect.Core
{
    public class GameManager
    {
        readonly int[,] _board = new int[3, 3];
        int _finalScore;

        DateTime _gameStartTime;
        bool _isGameActive;
        bool _isPlayerTurn;
        bool _waitingForPlayerTurn;

        public event Action OnGameOver;

        readonly UIManager _uiManager;
        
        public GameManager(UIManager uiManager)
        {
            _uiManager = uiManager;
            OnGameOver += uiManager.ShowGameOver;

            //TODO: Load exising game & best score if exist
            var currentScore = 0;
            var bestScore = 0;
            uiManager.Initialize(ReplayGame, CellClicked);
            uiManager.UpdateScore(currentScore, bestScore);
        }
        
        public async void Initialize()
        {
            await LoadNewGameAsync();
        }

        async UniTask LoadNewGameAsync(bool? isUserFirstTurn = null)
        {
            ResetBoard();
            _isGameActive = true;
            _finalScore = 0;
            _gameStartTime = DateTime.UtcNow;

            _isPlayerTurn = isUserFirstTurn ?? Random.value > 0.5f;
            await GameLoop();
        }

        async void ReplayGame()
        {
            // Reset and start a fresh game
            _uiManager.ResetCells();
            await LoadNewGameAsync();
        }

        async UniTask GameLoop()
        {
            while (_isGameActive)
            {
                _uiManager.UpdateTurnText(_isPlayerTurn);
                if (_isPlayerTurn)
                    await WaitForPlayerTurn();
                else
                    await ComputerTurn();

                if (CheckGameOver())
                {
                    EndGame();
                    return;
                }

                // Switch turns
                _isPlayerTurn = !_isPlayerTurn;
            }
        }

        async UniTask WaitForPlayerTurn()
        {
            while (!_waitingForPlayerTurn)
                await UniTask.Yield();
            _waitingForPlayerTurn = false;
        }
        
        void CellClicked(int row, int column)
        {
            if (MakePlayerMove(row, column))
            {
                // Move was valid; update visuals
                _uiManager.UpdateCellVisual(row, column, "X");
            }
            else
            {
                // Move invalid; do nothing or give feedback
            }
        }

        bool MakePlayerMove(int x, int y)
        {
            if (!_isGameActive || !_isPlayerTurn || _board[x, y] != 0)
                return false;

            _board[x, y] = 1;
            _waitingForPlayerTurn = true;
            return true;
        }

        async UniTask ComputerTurn()
        {
            await UniTask.Delay(Random.Range(500, 1500));
            MakeRandomComputerMove();
        }

        void MakeRandomComputerMove()
        {
            var emptyCells = new List<(int, int)>();
            for (var x = 0; x < 3; x++)
                for (var y = 0; y < 3; y++)
                    if (_board[x, y] == 0)
                        emptyCells.Add((x, y));

            if (emptyCells.Count <= 0) return;
            
            var choice = emptyCells[Random.Range(0, emptyCells.Count)];
            _board[choice.Item1, choice.Item2] = 2;
            _uiManager.UpdateCellVisual(choice.Item1, choice.Item2, "O"); // Update UI
        }

        bool CheckGameOver()
        {
            // Rows and columns
            for (var i = 0; i < 3; i++)
            {
                if (_board[i, 0] != 0 && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2]) return true;
                if (_board[0, i] != 0 && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i]) return true;
            }

            // Diagonals
            if (_board[0, 0] != 0 && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2]) return true;
            if (_board[0, 2] != 0 && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0]) return true;

            // Draw check
            foreach (var cell in _board)
                if (cell == 0)
                    return false;

            return true;
        }

        void EndGame()
        {
            _isGameActive = false;
            _finalScore = CalculateScore();
            _uiManager.ToggleCellsInteraction(false);
            OnGameOver?.Invoke();
        }

        int CalculateScore()
        {
            var elapsed = (float)(DateTime.UtcNow - _gameStartTime).TotalSeconds;
            var baseScore = 100 - Mathf.Clamp((int)elapsed, 0, 100);
            return baseScore;
        }

        public int GetFinalScore()
        {
            return _finalScore;
        }

        void ResetBoard()
        {
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                _board[x, y] = 0;
        }

        public int GetCellState(int x, int y)
        {
            return _board[x, y];
        }
    }
}