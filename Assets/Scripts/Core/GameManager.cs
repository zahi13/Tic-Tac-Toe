using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayPerfect.SaveSystem;
using PlayPerfect.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayPerfect.Core
{
    public class GameManager : IGameManager
    {
        readonly UIManager _uiManager;
        readonly StorageManager<GameState> _storageManager;
        
        int[,] _board = new int[3, 3];
        
        bool _isPlayerTurn;
        bool _waitingForPlayerTurn;
        float _totalReactionTime;
        int _bestScore;

        public enum GameResult { None, Win, Lose, Tie }
        public GameResult Result { get; private set; } = GameResult.None;
        public RowTypesContainer.RowType RowResult { get; private set; } = RowTypesContainer.RowType.None;
        
        public event Action OnGameOver;

        public bool IsGameInProgress { get; private set; }

        public GameManager(UIManager uiManager, StorageManager<GameState> storageManager)
        {
            _uiManager = uiManager;
            _storageManager = storageManager;
            
            uiManager.Initialize(this, ReplayGame, CellClicked);
            OnGameOver += uiManager.OnGameOverHandler;
            
            ApplicationEventsHandler.OnApplicationPauseEvent += _ => SaveGameState();
            ApplicationEventsHandler.OnApplicationQuitEvent += SaveGameState;
        }

        public async void Initialize()
        {
            bool? isUserFirstTurn = null;
            _loadedGameState = LoadGameState();
            if (_loadedGameState != null && _loadedGameState.IsGameInProgress)
                isUserFirstTurn = _loadedGameState.IsPlayerTurn;
            await LoadNewGameAsync(isUserFirstTurn);
        }

        async void ReplayGame()
        {
            await LoadNewGameAsync();
        }
        
        public async UniTask LoadNewGameAsync(bool? isUserFirstTurn = null)
        {
            IsGameInProgress = true;
            Result = GameResult.None;
            RowResult = RowTypesContainer.RowType.None;
            
            if (isUserFirstTurn == null)
                SetNewGame();
            if (_loadedGameState != null)
            {
                if (_loadedGameState.IsGameInProgress)
                {
                    _board = _loadedGameState.Board;
                    _isPlayerTurn = _loadedGameState.IsPlayerTurn;
                    _totalReactionTime = _loadedGameState.TotalReactionTime;
                    
                    _uiManager.SetBoard(_board);
                }
                _bestScore = Math.Max(_bestScore, _loadedGameState.TotalScore);
            }
            
            _uiManager.UpdateGameResultText(GameResult.None);
            _uiManager.UpdateScore(0, _bestScore);
            
            await GameLoop();
        }

        void SetNewGame()
        {
            _totalReactionTime = 0f;
            _isPlayerTurn = Random.value > 0.5f;
            ResetBoard();
            _uiManager.ResetCells();
        }
        
        void EndGame()
        {
            IsGameInProgress = false;
            OnGameOver?.Invoke();
            SaveGameState();
        }

        public int GetBestScore()
        {
            return _bestScore;
        }

        void ResetBoard()
        {
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                _board[x, y] = 0;
        }
        
        #region Game Logic
        async UniTask GameLoop()
        {
            while (IsGameInProgress)
            {
                _uiManager.UpdateTurnText(_isPlayerTurn);

                if (_isPlayerTurn)
                {
                    _uiManager.ToggleEmptyCellsInteraction(true, _board);
                    await WaitForPlayerTurn();
                }
                else
                {
                    _uiManager.ToggleEmptyCellsInteraction(false, _board);
                    await UniTask.Delay(Random.Range(1000, 3000));
                    MakeRandomComputerMove();
                }

                if (CheckGameOver())
                {
                    EndGame();
                    return;
                }

                // Switch turns
                _isPlayerTurn = !_isPlayerTurn;
            }
        }

        public async UniTask WaitForPlayerTurn()
        {
            var turnStartTime = DateTime.UtcNow;
            while (!_waitingForPlayerTurn)
                await UniTask.Yield();

            var turnReactionTime = (float)(DateTime.UtcNow - turnStartTime).TotalSeconds;
            _totalReactionTime += turnReactionTime;
            _waitingForPlayerTurn = false;
            Debug.Log($"Turn Reaction Time = {turnReactionTime} | Total Reaction Time = {_totalReactionTime}");
        }
        
        void CellClicked(int row, int column)
        {
            if (MakePlayerMove(row, column))
                _uiManager.UpdateCellVisual(row, column, AssetsNames.X_SPRITE_ASSET_NAME);
        }

        bool MakePlayerMove(int x, int y)
        {
            if (!IsGameInProgress || !_isPlayerTurn || _board[x, y] != 0)
                return false;

            _board[x, y] = 1;
            _waitingForPlayerTurn = true;
            return true;
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
            _uiManager.UpdateCellVisual(choice.Item1, choice.Item2, AssetsNames.O_SPRITE_ASSET_NAME);
        }

        bool CheckGameOver()
        {
            // Check rows and columns
            for (var i = 0; i < 3; i++)
            {
                if (_board[i, 0] != 0 && _board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2])
                {
                    Result = _board[i, 0] == 1 ? GameResult.Win : GameResult.Lose;
                    RowResult = i switch
                    {
                        0 => RowTypesContainer.RowType.HorizontalTop,
                        1 => RowTypesContainer.RowType.HorizontalMiddle,
                        2 => RowTypesContainer.RowType.HorizontalBottom,
                        _ => RowResult
                    };
                    return true;
                }
                if (_board[0, i] != 0 && _board[0, i] == _board[1, i] && _board[1, i] == _board[2, i])
                {
                    Result = _board[0, i] == 1 ? GameResult.Win : GameResult.Lose;
                    RowResult = i switch
                    {
                        0 => RowTypesContainer.RowType.VerticalLeft,
                        1 => RowTypesContainer.RowType.VerticalMiddle,
                        2 => RowTypesContainer.RowType.VerticalRight,
                        _ => RowResult
                    };
                    return true;
                }
            }

            // Check diagonals
            if (_board[0, 0] != 0 && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
            {
                Result = _board[0, 0] == 1 ? GameResult.Win : GameResult.Lose;
                RowResult = RowTypesContainer.RowType.ForwardSlash;
                return true;
            }
            if (_board[0, 2] != 0 && _board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0])
            {
                Result = _board[0, 2] == 1 ? GameResult.Win : GameResult.Lose;
                RowResult = RowTypesContainer.RowType.BackwardSlash;
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
            Result = GameResult.Tie;
            return true;
        }
        #endregion
        
        #region Loading/Saving
        
        const string STORAGE_KEY = "CurrentGame";
        GameState _loadedGameState;
        
        [Serializable]
        public class GameState
        {
            public bool IsGameInProgress;
            public int[,] Board;
            public bool IsPlayerTurn;
            public int TotalScore;
            public float TotalReactionTime;
        }
        
        void SaveGameState()
        {
            _bestScore = Math.Max(_bestScore, GetFinalScore());
            var state = new GameState
            {
                IsGameInProgress = IsGameInProgress,
                Board = _board,
                IsPlayerTurn = _isPlayerTurn,
                TotalScore = _bestScore,
                TotalReactionTime = _totalReactionTime,
            };

            _storageManager.Save(STORAGE_KEY, state);
        }

        GameState LoadGameState()
        {
            if (!_storageManager.HasKey(STORAGE_KEY)) return null;
            var savedState = _storageManager.Load(STORAGE_KEY);
            return savedState;
        }
        #endregion
        
        #region Scoring
        public int GetFinalScore()
        {
            if (IsGameInProgress) return 0;
            
            var elapsedTime = _totalReactionTime;
            int score = Result switch
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
        #endregion
    }
}