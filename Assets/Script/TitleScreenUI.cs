using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>タイトルから新しいランを開始する。戦闘シーンの状態は持ち越さない。</summary>
public sealed class TitleScreenUI : MonoBehaviour
{
    public const string ScenePath = "Assets/Scenes/TitleScene.unity";
    public const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    [SerializeField] private Button startButton;
    private bool isStarting;

    private void Awake()
    {
        Time.timeScale = 1f;
        startButton.onClick.AddListener(StartRun);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(StartRun);
    }

    public void StartRun()
    {
        if (isStarting) return;
        isStarting = true;
        startButton.interactable = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameplayScenePath);
    }
}
