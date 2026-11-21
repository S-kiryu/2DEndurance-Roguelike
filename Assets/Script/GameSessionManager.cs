using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>1ランの生存時間・ゲームオーバー・リトライを管理する。</summary>
[DefaultExecutionOrder(-100)]
public sealed class GameSessionManager : MonoBehaviour
{
    public event Action GameOver;

    public float SurvivalTime { get; private set; }
    public int ReachedWave { get; private set; } = 1;
    public bool IsGameOver { get; private set; }

    /// <summary>
    /// セッションをリセットする関数
    /// </summary>
    public void RestartRun()
    {
        if (_isLeavingScene || !IsGameOver) return;
        _isLeavingScene = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameObject.scene.path);
    }

    /// <summary>
    /// タイトルに戻る関数
    /// </summary>
    public void ReturnToTitle()
    {
        if (_isLeavingScene || !IsGameOver) return;
        _isLeavingScene = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(TitleScreenUI.ScenePath);
    }

    internal bool TryPauseForSkillSelection()
    {
        if (!isActiveAndEnabled || IsGameOver || _pausedForSkillSelection || Time.timeScale <= 0f)
        {
            return false;
        }

        _timeScaleBeforeSkillSelection = Time.timeScale;
        _pausedForSkillSelection = true;
        Time.timeScale = 0f;
        return true;
    }

    internal void ResumeAfterSkillSelection()
    {
        if (!_pausedForSkillSelection)
            return;

        _pausedForSkillSelection = false;
        if (!IsGameOver)
        {
            Time.timeScale = _timeScaleBeforeSkillSelection;
        }
    }

    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private WaveController _waveController;
    [SerializeField] private GameSessionUI _sessionUI;

    private bool _isLeavingScene;
    private bool _pausedForSkillSelection;
    private float _timeScaleBeforeSkillSelection;

    private void Awake()
    {
        Time.timeScale = 1f;
        SurvivalTime = 0f;
        ReachedWave = 1;
        IsGameOver = false;
    }

    private void Start()
    {
        if (_sessionUI != null)
        {
            _sessionUI.Initialize(this);
            RefreshUI();
        }
    }

    private void OnEnable()
    {
        if (_player != null)
        {
            _player.PlayerDied += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.PlayerDied -= OnPlayerDied;
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

    private void OnPlayerDied()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        ReachedWave = _waveController != null
            ? Mathf.Max(1, _waveController.ReachedWaveNumber)
            : 1;
        Time.timeScale = 0f;
        _sessionUI?.ShowGameOver(SurvivalTime, ReachedWave);
        GameOver?.Invoke();
    }

    /// <summary>
    /// UIを更新する関数
    /// </summary>
    private void RefreshUI()
    {
        if (_sessionUI == null || _player == null)
        {
            return;
        }

        int wave = _waveController != null
            ? Mathf.Max(1, _waveController.ReachedWaveNumber)
            : 1;
        float remaining = _waveController != null
            ? _waveController.WaveRemainingTime
            : 0f;

        _sessionUI.RefreshHud(
            _player.Model,
            SurvivalTime,
            wave,
            remaining
        );
    }
}
