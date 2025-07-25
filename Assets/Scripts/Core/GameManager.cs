using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayPerfect.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayPerfect.Core
{
    public class GameManager : IGameManager
    {
        readonly int[,] _board = new int[3, 3];

        readonly UIManager _uiManager;
        int _finalScore;

        DateTime _gameStartTime;
        bool _isPlayerTurn;
        bool _waitingForPlayerTurn;

        public enum GameResult { None, Win, Lose, Tie }
        GameResult _result = GameResult.None;

        public GameManager(UIManager uiManager)
        {
            _uiManager = uiManager;
            OnGameOver += uiManager.ShowGameOver;

            uiManager.Initialize(ReplayGame, CellClicked);
        }

        public event Action OnGameOver;

        public bool IsGameInProgress { get; private set; }

        public async UniTask LoadNewGameAsync(bool? isUserFirstTurn = null)
        {
            ResetBoard();
            IsGameInProgress = true;
            _gameStartTime = DateTime.UtcNow;

            _isPlayerTurn = isUserFirstTurn ?? Random.value > 0.5f;
            
            _uiManager.ResetCells();
            _uiManager.UpdateGameResultText(GameResult.None);
            
            // TODO: Load existing game & best score if exist
            var currentScore = 0;
            var bestScore = 0;
            _uiManager.UpdateScore(currentScore, bestScore);
            
            await GameLoop();
        }

        public async UniTask WaitForPlayerTurn()
        {
            while (!_waitingForPlayerTurn)
                await UniTask.Yield();

            _waitingForPlayerTurn = false;
        }

        public int GetFinalScore()
        {
            return _finalScore;
        }

        public async void Initialize()
        {
            await LoadNewGameAsync();
        }

        async void ReplayGame()
        {
            await LoadNewGameAsync();
        }

        async UniTask GameLoop()
        {
            while (IsGameInProgress)
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

        void CellClicked(int row, int column)
        {
            if (MakePlayerMove(row, column))
                _uiManager.UpdateCellVisual(row, column, "X");
        }

        bool MakePlayerMove(int x, int y)
        {
            if (!IsGameInProgress || !_isPlayerTurn || _board[x, y] != 0)
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

            if (emptyCells.Count == 0) return;

            var choice = emptyCells[Random.Range(0, emptyCells.Count)];
            _board[choice.Item1, choice.Item2] = 2;
            _uiManager.UpdateCellVisual(choice.Item1, choice.Item2, "O");
        }

        bool CheckGameOver()
        {
            // Check rows and columns
            for (var i = 0; i < 3; i++)
            {
                if (_board[i, 0] != 0 && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2])
                {
                    _result = _board[i, 0] == 1 ? GameResult.Win : GameResult.Lose;
                    return true;
                }
                if (_board[0, i] != 0 && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i])
                {
                    _result = _board[0, i] == 1 ? GameResult.Win : GameResult.Lose;
                    return true;
                }
            }

            // Check diagonals
            if (_board[0, 0] != 0 && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
            {
                _result = _board[0, 0] == 1 ? GameResult.Win : GameResult.Lose;
                return true;
            }
            if (_board[0, 2] != 0 && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
            {
                _result = _board[0, 2] == 1 ? GameResult.Win : GameResult.Lose;
                return true;
            }

            // Check for draw
            bool boardFull = true;
            foreach (var cell in _board)
            {
                if (cell != 0) continue;
                boardFull = false;
                break;
            }

            if (!boardFull) return false;
            _result = GameResult.Tie;
            return true;
        }

        void EndGame()
        {
            IsGameInProgress = false;
            
            _finalScore = CalculateScore();
            _uiManager.UpdateScore(_finalScore,0);
            _uiManager.ToggleCellsInteraction(false);
            _uiManager.UpdateGameResultText(_result);
            
            _result = GameResult.None;
  
            OnGameOver?.Invoke();
        }

        int CalculateScore()
        {
            float elapsedTime = (float)(DateTime.UtcNow - _gameStartTime).TotalSeconds;

            int score = _result switch
            {
                GameResult.Win => CalculateRangedScore(elapsedTime, 50, 100),
                GameResult.Tie => CalculateRangedScore(elapsedTime, 2, 49),
                GameResult.Lose => 1,
                _ => 1
            };

            return score;
        }

        int CalculateRangedScore(float elapsedTime, int minScore, int maxScore)
        {
            if (elapsedTime <= 10f)
                return maxScore;
            if (elapsedTime >= 20f)
                return minScore;

            float t = (elapsedTime - 10f) / 10f; // 0 to 1 between 10s and 20s
            return Mathf.RoundToInt(Mathf.Lerp(maxScore, minScore, t));
        }

        void ResetBoard()
        {
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                _board[x, y] = 0;
        }
    }
}