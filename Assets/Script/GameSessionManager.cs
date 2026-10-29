using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>1ランの生存時間・ゲームオーバー・リトライを管理する。</summary>
[DefaultExecutionOrder(-100)]
public sealed class GameSessionManager : MonoBehaviour
{
    public event Action GameOver;

    [SerializeField] private PlayerPresenter player;
    [SerializeField] private WaveController waveController;
    [SerializeField] private GameSessionUI sessionUI;

    public float SurvivalTime { get; private set; }
    public int ReachedWave { get; private set; } = 1;
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        SurvivalTime = 0f;
        ReachedWave = 1;
        IsGameOver = false;
    }

    private void Start()
    {
        if (sessionUI != null)
        {
            sessionUI.Initialize(this);
            RefreshUI();
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.PlayerDied += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.PlayerDied -= OnPlayerDied;
        }
    }

    private void Update()
    {
        if (!IsGameOver && Time.timeScale > 0f)
        {
            SurvivalTime += Time.deltaTime;
        }

        RefreshUI();
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnPlayerDied()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        ReachedWave = waveController != null
            ? Mathf.Max(1, waveController.ReachedWaveNumber)
            : 1;
        Time.timeScale = 0f;
        sessionUI?.ShowGameOver(SurvivalTime, ReachedWave);
        GameOver?.Invoke();
    }

    private void RefreshUI()
    {
        if (sessionUI == null || player == null)
        {
            return;
        }

        int wave = waveController != null
            ? Mathf.Max(1, waveController.ReachedWaveNumber)
            : 1;
        float remaining = waveController != null
            ? waveController.WaveRemainingTime
            : 0f;

        sessionUI.RefreshHud(
            player.Model.CurrentHealth,
            player.Model.MaxHealth,
            SurvivalTime,
            wave,
            remaining
        );
    }
}
