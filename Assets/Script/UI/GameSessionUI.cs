using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>戦闘HUDとゲームオーバー結果を表示する。</summary>
public sealed class GameSessionUI : MonoBehaviour
{
    [Header("戦闘HUD")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text survivalTimeText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text waveRemainingText;

    [Header("ゲームオーバー")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultTimeText;
    [SerializeField] private TMP_Text resultWaveText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

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
        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(ReturnToTitle);
            titleButton.onClick.AddListener(ReturnToTitle);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(Restart);
        }
        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(ReturnToTitle);
        }
    }

    /// <summary>
    /// UIを更新する関数
    /// </summary>
    /// <param name="currentHealth">現在の体力</param>
    /// <param name="maxHealth">最大体力</param>
    /// <param name="survivalTime">生存時間</param>
    /// <param name="wave">ウェーブ番号</param>
    /// <param name="waveRemaining">次のウェーブまでの残り時間</param>
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

    /// <summary>
    /// 死亡時のUI表示
    /// </summary>
    /// <param name="survivalTime">生き残った時間</param>
    /// <param name="reachedWave">到達ウェーブ数</param>
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
            gameOverPanel.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// タイトルに戻る関数
    /// </summary>
    private void ReturnToTitle()
    {
        session?.ReturnToTitle();
    }

    /// <summary>
    /// リスタート関数
    /// </summary>
    private void Restart()
    {
        session?.RestartRun();
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
