using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>戦闘HUDとゲームオーバー結果を表示する。</summary>
public sealed class GameSessionUI : MonoBehaviour
{
    [Header("戦闘HUD")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text survivalTimeText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text waveRemainingText;

    [Header("ゲームオーバー")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text resultTimeText;
    [SerializeField] private Text resultWaveText;
    [SerializeField] private Button retryButton;

    private GameSessionManager session;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void Initialize(GameSessionManager manager)
    {
        session = manager;
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(Restart);
            retryButton.onClick.AddListener(Restart);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(Restart);
        }
    }

    public void RefreshHud(
        int currentHealth,
        int maxHealth,
        float survivalTime,
        int wave,
        float waveRemaining)
    {
        if (healthText != null)
        {
            healthText.text = $"HP {currentHealth} / {maxHealth}";
        }
        if (survivalTimeText != null)
        {
            survivalTimeText.text = $"TIME {FormatTime(survivalTime)}";
        }
        if (waveText != null)
        {
            waveText.text = $"WAVE {wave}";
        }
        if (waveRemainingText != null)
        {
            waveRemainingText.text = $"NEXT {Mathf.Max(0f, waveRemaining):0.0}s";
        }
    }

    public void ShowGameOver(float survivalTime, int reachedWave)
    {
        if (resultTimeText != null)
        {
            resultTimeText.text = $"生存時間  {FormatTime(survivalTime)}";
        }
        if (resultWaveText != null)
        {
            resultWaveText.text = $"到達ウェーブ  {reachedWave}";
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void Restart()
    {
        session?.RestartRun();
    }

    private static string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }
}
