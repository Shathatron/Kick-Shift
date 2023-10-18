using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using System;

namespace Moball {
    public class GameUi : MonoBehaviour {
        [Serializable]
        struct CountDownItem {
            public string Text;
            public Color Color;
            public static readonly CountDownItem Default = new CountDownItem {
                Text = "?",
                Color = Color.white
            };
        }

        [Header("Scoreboard")]
        [SerializeField, Required] TextMeshProUGUI _team0ScoreText = default;
        [SerializeField, Required] TextMeshProUGUI _team1ScoreText = default;
        [SerializeField, Required] TextMeshProUGUI _timerText = default;
        [SerializeField] Color _overtimeTimerTextColor = Color.white;
        [SerializeField, Required] Image _team0ScoreBackground = default;
        [SerializeField, Required] Image _team1ScoreBackground = default;
        [SerializeField, Required] RectTransform _overtimePanel = default;

        [Header("Center message")]
        [SerializeField, Required] Canvas _centerMessageCanvas = default;
        [SerializeField, Required] TextMeshProUGUI _centerMessageText = default;

        [Header("Center message")]
        [SerializeField, Required] Canvas _countDownCanvas = default;
        [SerializeField, Required] TextMeshProUGUI _countDownText = default;
        [SerializeField] CountDownItem[] _countDownItems = ArrayUtility.CreateFilled(3, CountDownItem.Default);

        int[] _scoresValue;
        int _timerSecondsValue;
        Color _defaultCenterMessageColor;
        Color _defaultTimerTextColor;

        // -- Public interface --

        public void Initialize(
            IReadOnlyList<int> teamScores,
            float timeRemaining,
            Color team0Color,
            Color team1Color
        ) {
            _scoresValue = new int[teamScores.Count];
            for (var i = 0; i < _scoresValue.Length; i++) {
                SetScoreText(i, _scoresValue[i]);
            }
            timeRemaining = Mathf.CeilToInt(timeRemaining);
            SetTimerText(_timerSecondsValue);
            _team0ScoreBackground.color = team0Color;
            _team1ScoreBackground.color = team1Color;
            HideCountDown();
        }

        public void ShowCountDown(int secondsRemaining) {
            if (secondsRemaining < 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(secondsRemaining),
                    secondsRemaining,
                    "Invalid count down index"
                );
            }
            if (_countDownItems.Length > secondsRemaining) {
                _countDownCanvas.gameObject.SetActive(true);
                ref var item = ref _countDownItems[secondsRemaining];
                _countDownText.text = item.Text;
                _countDownText.color = item.Color;
            } else {
                Debug.LogError($"No count down item for {secondsRemaining}", this);
            }
        }

        public void HideCountDown() {
            _countDownCanvas.gameObject.SetActive(false);
        }

        public void SetScores(IReadOnlyList<int> scores) {
            for (var i = 0; i < scores.Count; i++) {
                if (scores[i] != _scoresValue[i]) {
                    SetScoreText(i, scores[i]);
                }
            }
        }

        public void SetIsOvertime(bool isOvertime) {
            _timerText.color = isOvertime
                ? _overtimeTimerTextColor
                : _defaultTimerTextColor;
            _overtimePanel.gameObject.SetActive(isOvertime);
        }

        public void SetTimeRemaining(float timeRemaining) {
            int secondsRemaining = Mathf.CeilToInt(timeRemaining);
            if (secondsRemaining != _timerSecondsValue) {
                SetTimerText(secondsRemaining);
            }
        }

        public void ShowCenterMessage(string text) =>
            ShowCenterMessage(text, _defaultCenterMessageColor);

        public void ShowCenterMessage(string text, Color color) {
            _centerMessageCanvas.gameObject.SetActive(true);
            _centerMessageText.color = color;
            _centerMessageText.text = text;
        }

        public void HideCenterMessage() {
            _centerMessageCanvas.gameObject.SetActive(false);
        }

        // -- Unity messages --

        void Awake() {
            HideCenterMessage();
            _defaultCenterMessageColor = _centerMessageText.color;
            _defaultTimerTextColor = _timerText.color;
        }

        // -- Private implementation --

        // Generates string garbage.
        void SetScoreText(int teamId, int score) {
            switch (teamId) {
                case 0: _team0ScoreText.text = score.ToString(); break;
                case 1: _team1ScoreText.text = score.ToString(); break;
                default:
                    DebugUtility.AssertNoMatch(teamId, nameof(teamId));
                    return;
            }
            _scoresValue[teamId] = score;
        }

        // Generates string garbage.
        void SetTimerText(int timerSeconds) {
            timerSeconds = Mathf.Abs(timerSeconds);
            var minutes = timerSeconds / 60;
            var seconds = timerSeconds % 60;
            _timerText.text = $"{minutes:D2}:{seconds:D2}";
            _timerSecondsValue = timerSeconds;
        }
    }
}
