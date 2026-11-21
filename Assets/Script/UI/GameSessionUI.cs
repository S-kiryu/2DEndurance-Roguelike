using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>戦闘HUDとゲームオーバー結果を表示する。</summary>
public sealed class GameSessionUI : MonoBehaviour
{
    public void Initialize(GameSessionManager manager)
    {
        _session = manager;
        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(Restart);
            _retryButton.onClick.AddListener(Restart);
        }
        if (_titleButton != null)
        {
            _titleButton.onClick.RemoveListener(ReturnToTitle);
            _titleButton.onClick.AddListener(ReturnToTitle);
        }
    }

    /// <summary>
    /// UIを更新する関数
    /// </summary>
    /// <param name="player">プレイヤーのHPと経験値</param>
    /// <param name="survivalTime">生存時間</param>
    /// <param name="wave">ウェーブ番号</param>
    /// <param name="waveRemaining">次のウェーブまでの残り時間</param>
    public void RefreshHud(
        PlayerModel player,
        float survivalTime,
        int wave,
        float waveRemaining)
    {
        if (_healthText != null)
        {
            _healthText.text = $"HP {player.CurrentHealth} / {player.MaxHealth}";
        }
        if (_healthFill != null)
        {
            float healthRatio = player.MaxHealth > 0
                ? Mathf.Clamp01((float)player.CurrentHealth / player.MaxHealth)
                : 0f;
            _healthFill.rectTransform.anchorMax = new Vector2(healthRatio, 1f);
        }
        if (_experienceText != null)
        {
            _experienceText.text = $"LV {player.Level}   XP {player.CurrentExperience} / {player.ExperienceToNextLevel}";
        }
        if (_survivalTimeText != null)
        {
            _survivalTimeText.text = $"TIME {FormatTime(survivalTime)}";
        }
        if (_waveText != null)
        {
            _waveText.text = $"WAVE {wave}";
        }
        if (_waveRemainingText != null)
        {
            _waveRemainingText.text = $"NEXT {Mathf.Max(0f, waveRemaining):0.0}s";
        }
    }

    /// <summary>
    /// 死亡時のUI表示
    /// </summary>
    /// <param name="survivalTime">生き残った時間</param>
    /// <param name="reachedWave">到達ウェーブ数</param>
    public void ShowGameOver(float survivalTime, int reachedWave)
    {
        if (_resultTimeText != null)
        {
            _resultTimeText.text = $"生存時間  {FormatTime(survivalTime)}";
        }
        if (_resultWaveText != null)
        {
            _resultWaveText.text = $"到達ウェーブ  {reachedWave}";
        }
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
            _gameOverPanel.transform.SetAsLastSibling();
        }
    }

    [Header("戦闘HUD")]
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private Image _healthFill;
    [SerializeField] private TMP_Text _survivalTimeText;
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _waveRemainingText;
    [SerializeField] private TMP_Text _experienceText;

    [Header("ゲームオーバー")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TMP_Text _resultTimeText;
    [SerializeField] private TMP_Text _resultWaveText;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _titleButton;

    private GameSessionManager _session;

    private void Awake()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(Restart);
        }
        if (_titleButton != null)
        {
            _titleButton.onClick.RemoveListener(ReturnToTitle);
        }
    }

    /// <summary>
    /// タイトルに戻る関数
    /// </summary>
    private void ReturnToTitle()
    {
        _session?.ReturnToTitle();
    }

    /// <summary>
    /// リスタート関数
    /// </summary>
    private void Restart()
    {
        _session?.RestartRun();
    }

    /// <summary>
    /// タイマーのフォーマット関数
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }
}
