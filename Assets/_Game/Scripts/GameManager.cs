using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Moball {
    class GameManager : MonoBehaviour {
#pragma warning disable 414
        [InfoBox("F2 - Restart game\nF3 - Join all players\nF4 - Unjoin all players\n1-4 - Toggle specific player")]

        [Header("Rules")]
        [SerializeField] GameConfig _gameConfig = null;
        [Tooltip("Maximum game length in seconds")]
        [SerializeField] float _gameDuration = 5 * 60f;
        [SerializeField] float _goalCelebrationDuration = 5f;
        [Tooltip("Time before countdown")]
        [SerializeField] float _countDownDelay = 2f;
        [SerializeField] float _gameOverCelebrationDuration = 10f;
        [SerializeField] float _overtimeMessageDuration = 2f;

        [Header("Config")]
        [SerializeField] bool _skipCountdown = false;
        [Layer, SerializeField] int[] _playerCameraLayers = default;
        [SerializeField] TeamConfig[] _teamConfigs = default;

        [Header("Prefabs")]
        [SerializeField] Ball _ballPrefab = default;

        [Header("Reference")]
        [SerializeField, Required] Arena _arena = default;
        [SerializeField, Required] PlayerInputManager _playerInputManager = default;
        [SerializeField, Required] Transform _ballSpawnPoint = default;
        [SerializeField, Required] GameUi _gameUi = default;

        enum GameState {
            WaitingForRound,
            PlayingRound,
            GoalCelebration,
            GameOverCelebration
        }

        const int TeamCount = 2;

        Player[] _players;
        Ball _ball = default;
        float _elapsedGameTime = 0f;
        int[] _teamScores = new int[TeamCount];
        List<PlayerSpawnPoint>[] _playerSpawnPoints = ArrayUtility.CreateFilled(
            TeamCount,
            _ => new List<PlayerSpawnPoint>()
        );
        int[] _teamIdByPlayerId;
        GameState _gameState;
        CancellationTokenSource _gameCts;
        bool _isOvertime = false;

        List<PlayerSpawnPoint> _playerSpawnPointsTemp = new List<PlayerSpawnPoint>();

        // -- Unity messages --

        void Awake() {
            _players = new Player[_playerInputManager.maxPlayerCount];
            _ball = Instantiate(_ballPrefab);
            _ball.Config = _gameConfig.BallConfig;
            _teamIdByPlayerId = ArrayUtility.CreateFilled(_playerInputManager.maxPlayerCount, -1);
            InitializeSpawnPoints();
            for (var i = 0; i < _teamConfigs.Length; i++) {
                _arena.SetColors(i, _teamConfigs[i].ArenaColor);
            }

            UniTaskScheduler.UnobservedTaskException += e => Debug.LogError(e);
        }

        void FixedUpdate() {
            if (_gameState == GameState.PlayingRound) {
                _elapsedGameTime += Time.deltaTime;
                if (!_isOvertime && _elapsedGameTime >= _gameDuration) {
                    if (_teamScores.AllElementsEqual()) {
                        OvertimeAsync(NextGameCancellationToken()).Forget();
                    } else {
                        GameOverAsync(NextGameCancellationToken()).Forget();
                    }
                }
            }
        }

        void Update() {
            _gameUi.SetTimeRemaining(_gameDuration - _elapsedGameTime);
            _gameUi.SetIsOvertime(_isOvertime);

#if UNITY_EDITOR
            if (Keyboard.current.f2Key.wasPressedThisFrame) {
                StartGame();
            }
            if (Keyboard.current.f3Key.wasPressedThisFrame) {
                JoinAllPlayers();
            }
            if (Keyboard.current.f4Key.wasPressedThisFrame) {
                UnjoinAllPlayers();
            }
            if (Keyboard.current.digit1Key.wasPressedThisFrame) TogglePlayerJoined(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) TogglePlayerJoined(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) TogglePlayerJoined(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) TogglePlayerJoined(3);
#endif
        }

        // -- Event handlers --

        public void OnGoalScored(Goal goal) {
            if (_gameState == GameState.PlayingRound) {
                _teamScores[goal.ScoringTeamId]++;
                _gameUi.SetScores(_teamScores);
                goal.Explode();
                var teamConfig = GetTeamConfig(goal.ScoringTeamId);
                GoalAsync(teamConfig, NextGameCancellationToken()).Forget();
            }
        }

        public async void OnPlayerJoined(PlayerInput playerInput) {
            var index = playerInput.playerIndex;
            if (index >= _playerCameraLayers.Length) {
                Debug.LogError($"Player index {index} exceeds configured camera layers {_playerCameraLayers.Length}");
            } else if (playerInput.TryGetComponent(out Player player)) {
                _players[index] = player;
                player.CarConfig = _gameConfig.CarConfig;
                player.StaminaConfig = _gameConfig.StaminaConfig;
            } else {
                Debug.LogError($"Player had no {nameof(Player)} component", playerInput);
            }

            // Delay one frame to allow new object to initialize.
            // https://forum.unity.com/threads/playerinputmanager-onplayerjoined-is-called-before-awake-is-run-on-instantiated-prefab.1163554/
            await UniTask.WaitForEndOfFrame();
            StartGame();
        }

        // -- Private implementation --

        // Player management

        void TogglePlayerJoined(int index) {
            if (_players[index]) UnjoinPlayer(index);
            else JoinPlayer(index);
        }

        void JoinPlayer(int playerId) {
            if (_players[playerId]) {
                Debug.LogWarning($"Player {playerId} already joined");
            } else {
                _playerInputManager.JoinPlayer(playerId);
                Debug.Log($"Joined player {playerId}");
            }
        }

        void JoinAllPlayers() {
            for (var i = 0; i < _playerInputManager.maxPlayerCount; i++) {
                _playerInputManager.JoinPlayer(i);
            }
        }

        void UnjoinAllPlayers() {
            for (var i = 0; i < _players.Length; i++) {
                UnjoinPlayer(i);
            }
        }

        void UnjoinPlayer(int playerId) {
            var player = _players[playerId];
            if (player) {
                Destroy(player.transform.root.gameObject);
                Debug.Log($"Unjoined player {playerId}");
            } else {
                Debug.LogWarning($"Player {playerId} is not joined");
            }
        }

        // Game management

        void StartGame() {
            StartGameAsync(NextGameCancellationToken()).Forget();
        }

        void SetPlayersInputEnabled(bool isEnabled) {
            for (var i = 0; i < _players.Length; i++) {
                var player = _players[i];
                if (player) player.IsInputEnabled = isEnabled;
            }
        }

        async UniTask StartGameAsync(CancellationToken cancellationToken) {
            ArrayUtility.Fill(_teamScores, 0);
            _elapsedGameTime = 0;
            _isOvertime = false;
            _gameUi.Initialize(
                _teamScores,
                _gameDuration - _elapsedGameTime,
                _teamConfigs[0].UiColor,
                _teamConfigs[1].UiColor
            );
            await NextRoundAsync(cancellationToken);
        }

        async UniTask NextRoundAsync(CancellationToken cancellationToken) {
            ResetForRound();
            if (_skipCountdown) {
                StartRound();
            } else {
                await CountDownAndStartRoundAsync(cancellationToken);
            }
        }

        void ResetForRound() {
            ResetPlayers();
            ResetBall();
            SetPlayersInputEnabled(false);
            _gameState = GameState.WaitingForRound;
        }

        void StartRound() {
            SetPlayersInputEnabled(true);
            _gameState = GameState.PlayingRound;
        }

        async UniTask CountDownAndStartRoundAsync(CancellationToken cancellationToken) {
            await UniTask.Delay(
                TimeSpan.FromSeconds(_countDownDelay),
                cancellationToken: cancellationToken
            );
            try {
                for (var i = 3; i > 0; i--) {
                    _gameUi.ShowCountDown(i);
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(1),
                        cancellationToken: cancellationToken
                    );
                }
                _gameUi.ShowCountDown(0);
                StartRound();
                await UniTask.Delay(
                    TimeSpan.FromSeconds(1),
                    cancellationToken: cancellationToken
                );
            } finally {
                _gameUi.HideCountDown();
            }
        }

        void ResetBall() {
            _ball.transform.position = _ballSpawnPoint.position;
            _ball.Stop();
        }

        static int[] _teamCountsTemp = new int[TeamCount];
        void ResetPlayers() {
            ArrayUtility.Fill(_teamIdByPlayerId, -1);
            ArrayUtility.Fill(_teamCountsTemp, 0);
            var nextTeamId = 0;
            for (var i = 0; i < _players.Length; i++) {
                var player = _players[i];
                if (!player) continue;
                _teamIdByPlayerId[i] = nextTeamId;
                ConfigurePlayerCamera(i, nextTeamId, player);
                ref var teamCount = ref _teamCountsTemp[nextTeamId];
                var teamConfig = _teamConfigs[nextTeamId];
                player.Appearance.SetColors(
                    teamConfig.CarColor,
                    teamConfig.TryGetAccentColor(teamCount, out var accent) ? accent : Color.white
                );
                var spawnPoint = _playerSpawnPoints[nextTeamId][teamCount];
                player.Spawn(
                    position: spawnPoint.SpawnPosition,
                    rotation: nextTeamId == 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0)
                );
                player.CarGroup.ForceFollowCar = false;
                player.CarGroup.Ball = _ball;
                player.transform.root.name = $"Player {i}";
                teamCount++;
                nextTeamId = (nextTeamId + 1) % TeamCount;
            }
        }

        async UniTask OvertimeAsync(CancellationToken cancellationToken) {
            Debug.Assert(!_isOvertime, "Already in overtime");
            try {
                _isOvertime = true;
                ResetForRound();
                _gameUi.ShowCenterMessage("Overtime");
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_overtimeMessageDuration),
                    cancellationToken: cancellationToken
                );
            } finally {
                _gameUi.HideCenterMessage();
            }
            await CountDownAndStartRoundAsync(cancellationToken);
        }


        async UniTask GoalAsync(
            TeamConfig scoringTeamConfig,
            CancellationToken cancellationToken
        ) {
            try {
                for (var i = 0; i < _players.Length; i++) {
                    var player = _players[i];
                    if (player) player.CarGroup.ForceFollowCar = true;
                }
                _gameState = GameState.GoalCelebration;
                _gameUi.ShowCenterMessage(
                    $"{scoringTeamConfig.DisplayName} scores!",
                    scoringTeamConfig.UiColor
                );
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_goalCelebrationDuration),
                    cancellationToken: cancellationToken
                );
            } finally {
                _gameUi.HideCenterMessage();
            }
            if (_elapsedGameTime < _gameDuration) {
                await NextRoundAsync(cancellationToken);
            } else {
                await GameOverAsync(cancellationToken);
            }
        }

        async UniTask GameOverAsync(CancellationToken cancellationToken) {
            _gameState = GameState.GameOverCelebration;
            var winnerTeamId = ArrayUtility.MaxIndex(_teamScores);
            var isDraw = false;
            for (var i = 0; i < _teamScores.Length; i++) {
                if (
                    i != winnerTeamId &&
                    _teamScores[i] == _teamScores[winnerTeamId]
                ) {
                    isDraw = true;
                    break;
                }
            }

            try {
                if (isDraw) {
                    _gameUi.ShowCenterMessage("Draw!");
                } else {
                    var winner = _teamConfigs[winnerTeamId];
                    _gameUi.ShowCenterMessage(
                        $"{winner.DisplayName} wins!",
                        winner.UiColor
                    );
                }

                await UniTask.Delay(
                    TimeSpan.FromSeconds(_gameOverCelebrationDuration),
                    cancellationToken: cancellationToken
                );
            } finally {
                _gameUi.HideCenterMessage();
            }

            StartGame();
        }

        CancellationToken NextGameCancellationToken() {
            AsyncUtility.CancelAndDispose(ref _gameCts);
            _gameCts = new CancellationTokenSource();
            return _gameCts.Token;
        }

        // Cameras

        void ConfigurePlayerCamera(int index, int teamId, Player player) {
            player.transform.root.localRotation = teamId switch {
                0 => Quaternion.identity,
                _ => Quaternion.Euler(0, 180, 0)
            };
            player.CarGroup.ConfigureCamera(cameraLayer: _playerCameraLayers[index]);
        }

        // Initialization

        void InitializeSpawnPoints() {
            _arena.GetComponentsInChildren<PlayerSpawnPoint>(_playerSpawnPointsTemp);
            for (var i = 0; i < _playerSpawnPointsTemp.Count; i++) {
                var point = _playerSpawnPointsTemp[i];
                var teamId = point.TeamId;
                if (teamId < 0 || teamId >= _playerSpawnPoints.Length) {
                    Debug.LogError(
                        $"Unexpected team ID {teamId} for {nameof(PlayerSpawnPoint)}",
                        point
                    );
                    continue;
                }
                _playerSpawnPoints[teamId].Add(point);
            }
            for (var i = 0; i < _playerSpawnPoints.Length; i++) {
                var points = _playerSpawnPoints[i];
                points.Sort((a, b) => a.Id.CompareTo(b.Id));
                var prevId = -1;
                for (var j = 0; j < points.Count; j++) {
                    var expected = prevId + 1;
                    var point = points[j];
                    if (point.Id != expected) {
                        Debug.LogError($"Expected {nameof(PlayerSpawnPoint)} ID {expected}, got {point.Id}");
                        continue;
                    }
                    prevId = point.Id;
                }
            }
        }

        // Teams

        TeamConfig GetTeamConfig(int teamId) {
            if (teamId < 0 || teamId >= _teamConfigs.Length) {
                throw new ArgumentOutOfRangeException($"Invalid team ID: {teamId}", nameof(teamId));
            }
            return _teamConfigs[teamId];
        }
    }
}
