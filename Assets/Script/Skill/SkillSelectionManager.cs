using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>適用先まで確定した候補から、重複設定に従って最大3つを抽選する。</summary>
public sealed class SkillSelectionManager : MonoBehaviour
{
    //選択肢の数
    public const int ChoiceCount = 3;

    [SerializeField] private PlayerPresenter player;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GameSessionManager session;
    [SerializeField] private List<SkillBase> skillPool = new();

    [Header("シーンのCanvas上にある選択UI")]
    [SerializeField] private SkillSelectionUI selectionUI;

    private readonly List<SkillCandidate> currentChoices = new();
    private readonly List<SkillCandidate> availableCandidates = new();
    private readonly HashSet<string> candidateKeys = new();
    private SkillContext context;
    private PlayerInputReader playerInput;
    private int pendingSelections;

    private void Start()
    {
        if (session == null)
        {
            Debug.LogError("SkillSelectionManagerにシーン上のGameSessionManagerを設定してください。", this);
            enabled = false;
            return;
        }

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
        if (player != null)
        {
            player.Model.LeveledUp += OnPlayerLeveledUp;
            player.PlayerDied += CancelSelection;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.Model.LeveledUp -= OnPlayerLeveledUp;
            player.PlayerDied -= CancelSelection;
        }

        CancelSelection();
    }

    private void OnPlayerLeveledUp(int levelsGained)
    {
        pendingSelections += levelsGained;
        OpenNextSelection();
    }

    /// <summary>
    /// 選択肢を表示させる処理
    /// </summary>
    private void OpenNextSelection()
    {
        if (pendingSelections == 0 || !isActiveAndEnabled || IsChoosing ||
            player == null || !player.Model.IsAlive || session == null)
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
            pendingSelections = 0;
            Debug.LogWarning("適用可能なスキルがありません。", this);
            return;
        }

        if (!session.TryPauseForSkillSelection())
        {
            currentChoices.Clear();
            return;
        }

        pendingSelections--;
        IsChoosing = true;
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

        CloseSelection();
        SkillSelected?.Invoke(selected.Skill);
        OpenNextSelection();
        return true;
    }

    /// <summary>
    /// セレクトをキャンセルする処理
    /// </summary>
    public void CancelSelection()
    {
        pendingSelections = 0;
        CloseSelection();
    }

    private void CloseSelection()
    {
        if (!IsChoosing)
        {
            return;
        }

        IsChoosing = false;
        currentChoices.Clear();
        playerInput?.ClearJumpRequest();
        if (session != null)
        {
            session.ResumeAfterSkillSelection();
        }
        ChoicesClosed?.Invoke();
    }
}
