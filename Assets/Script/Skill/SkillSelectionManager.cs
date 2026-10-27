using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>候補から重複なしで最大3つを抽選する</summary>
public sealed class SkillSelectionManager : MonoBehaviour
{
    public const int ChoiceCount = 3;

    [SerializeField] private PlayerPresenter player;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private WaveController waveController;
    [SerializeField] private List<SkillBase> skillPool = new();

    [Header("シーンのCanvas上にある選択UI")]
    [SerializeField] private SkillSelectionUI selectionUI;

    private readonly List<SkillBase> currentChoices = new();
    private SkillContext context;
    private PlayerInputReader playerInput;
    private float previousTimeScale;

    private void Start()
    {
        if (selectionUI == null)
        {
            Debug.LogError("SkillSelectionManagerにシーン上のSkillSelectionUIを設定してください。", this);
            enabled = false;
            return;
        }

        // 画面はシーンのCanvasに配置し、ここでは既存UIと接続する。
        if (!selectionUI.Initialize(this))
        {
            enabled = false;
        }
    }

    public event Action ChoicesOpened;
    public event Action<SkillBase> SkillSelected;
    public event Action ChoicesClosed;

    public bool IsChoosing { get; private set; }
    public IReadOnlyList<SkillBase> CurrentChoices => currentChoices;

    private void Awake()
    {
        context = new SkillContext(player, weaponManager);
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInputReader>();
        }
    }

    private void OnEnable()
    {
        if (waveController != null)
        {
            waveController.WaveEnded += OnWaveEnded;
        }

        if (player != null)
        {
            player.PlayerDied += CancelSelection;
        }
    }

    private void OnDisable()
    {
        if (waveController != null)
        {
            waveController.WaveEnded -= OnWaveEnded;
        }

        if (player != null)
        {
            player.PlayerDied -= CancelSelection;
        }

        CancelSelection();
    }

    private void OnWaveEnded(int waveNumber)
    {
        OpenSelection();
    }

    public void OpenSelection()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsChoosing ||
            player == null || !player.Model.IsAlive || Time.timeScale <= 0f)
        {
            return;
        }

        currentChoices.Clear();
        foreach (SkillBase skill in skillPool)
        {
            if (skill != null && !currentChoices.Contains(skill) && skill.CanApply(context))
            {
                currentChoices.Add(skill);
            }
        }

        // 部分Fisher-Yates。候補数が増えても均等に、同じスキルを重複させず抽選する。
        int choiceCount = Mathf.Min(ChoiceCount, currentChoices.Count);
        for (int i = 0; i < choiceCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, currentChoices.Count);
            SkillBase temporary = currentChoices[i];
            currentChoices[i] = currentChoices[randomIndex];
            currentChoices[randomIndex] = temporary;
        }

        if (currentChoices.Count > choiceCount)
        {
            currentChoices.RemoveRange(choiceCount, currentChoices.Count - choiceCount);
        }

        if (currentChoices.Count == 0)
        {
            Debug.LogWarning("適用可能なスキルがありません。", this);
            return;
        }

        previousTimeScale = Time.timeScale;
        IsChoosing = true;
        Time.timeScale = 0f;
        playerInput?.ClearJumpRequest();
        ChoicesOpened?.Invoke();
    }

    /// <param name="index">0始まりの選択肢番号。</param>
    public bool SelectChoice(int index)
    {
        if (!IsChoosing || index < 0 || index >= currentChoices.Count ||
            player == null || !player.Model.IsAlive)
        {
            return false;
        }

        SkillBase selected = currentChoices[index];
        if (selected == null || !selected.TryApply(context))
        {
            Debug.LogWarning("スキルを適用できませんでした。別の候補を選んでください。", this);
            return false;
        }

        CancelSelection();
        SkillSelected?.Invoke(selected);
        return true;
    }

    public void CancelSelection()
    {
        if (!IsChoosing)
        {
            return;
        }

        IsChoosing = false;
        currentChoices.Clear();
        playerInput?.ClearJumpRequest();
        Time.timeScale = previousTimeScale;
        ChoicesClosed?.Invoke();
    }
}
