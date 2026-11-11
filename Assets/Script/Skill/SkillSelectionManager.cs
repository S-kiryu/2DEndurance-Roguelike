using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>適用先まで確定した候補から、重複設定に従って最大3つを抽選する。</summary>
public sealed class SkillSelectionManager : MonoBehaviour
{
    public const int ChoiceCount = 3;

    [SerializeField] private PlayerPresenter player;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private WaveController waveController;
    [SerializeField] private List<SkillBase> skillPool = new();

    [Header("シーンのCanvas上にある選択UI")]
    [SerializeField] private SkillSelectionUI selectionUI;

    private readonly List<SkillCandidate> currentChoices = new();
    private readonly List<SkillCandidate> availableCandidates = new();
    private readonly HashSet<string> candidateKeys = new();
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
    public IReadOnlyList<SkillCandidate> CurrentChoices => currentChoices;

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

    /// <summary>
    /// ウェーブが終わった時の処理
    /// </summary>
    /// <param name="waveNumber">ウェーブ数</param>
    private void OnWaveEnded(int waveNumber)
    {
        OpenSelection();
    }

    /// <summary>
    /// 選択肢を表示させる処理
    /// </summary>
    public void OpenSelection()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsChoosing ||
            player == null || !player.Model.IsAlive || Time.timeScale <= 0f)
        {
            return;
        }

        currentChoices.Clear();
        availableCandidates.Clear();
        candidateKeys.Clear();

        foreach (SkillBase skill in skillPool)
        {
            if (skill == null)
            {
                continue;
            }

            var generated = new List<SkillCandidate>();
            skill.BuildCandidates(context, generated);
            foreach (SkillCandidate candidate in generated)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.IdentityKey) ||
                    !candidateKeys.Add(candidate.IdentityKey))
                {
                    continue;
                }

                availableCandidates.Add(candidate);
            }
        }

        for (int i = 0; i < ChoiceCount && availableCandidates.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCandidates.Count);
            SkillCandidate selected = availableCandidates[randomIndex];
            currentChoices.Add(selected);

            if (!selected.AllowDuplicateInSameSelection)
            {
                availableCandidates.RemoveAt(randomIndex);
            }
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

        SkillCandidate selected = currentChoices[index];
        if (selected == null || !selected.TryApply(context))
        {
            Debug.LogWarning("スキルを適用できませんでした。別の候補を選んでください。", this);
            return false;
        }

        CancelSelection();
        SkillSelected?.Invoke(selected.Skill);
        return true;
    }

    /// <summary>
    /// セレクトをキャンセルする処理
    /// </summary>
    public void CancelSelection()
    {
        if (!IsChoosing)
        {
            return;
        }

        IsChoosing = false;
        currentChoices.Clear();
        playerInput?.ClearJumpRequest();
        bool playerIsDead = player != null && !player.Model.IsAlive;
        Time.timeScale = playerIsDead ? 0f : previousTimeScale;
        ChoicesClosed?.Invoke();
    }
}
