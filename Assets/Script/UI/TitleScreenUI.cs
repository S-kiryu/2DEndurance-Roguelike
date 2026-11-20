using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>タイトルから新しいランを開始する。戦闘シーンの状態は持ち越さない。</summary>
public sealed class TitleScreenUI : MonoBehaviour
{
    public const string ScenePath = "Assets/Scenes/TitleScene.unity";
    public const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    public void StartRun()
    {
        if (_isStarting) return;
        _isStarting = true;
        _startButton.interactable = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameplayScenePath);
    }

    [SerializeField] private Button _startButton;
    private bool _isStarting;

    private void Awake()
    {
        Time.timeScale = 1f;
        _startButton.onClick.AddListener(StartRun);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(StartRun);
    }
}
