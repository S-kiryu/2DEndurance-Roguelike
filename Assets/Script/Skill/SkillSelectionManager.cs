using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>適用先まで確定した候補から、重複設定に従って最大3つを抽選する。</summary>
public sealed class SkillSelectionManager : MonoBehaviour
{
    //選択肢の数
    public const int ChoiceCount = 3;

    public event Action ChoicesOpened;
    public event Action ChoicesChanged;
    public event Action<SkillBase> SkillSelected;
    public event Action ChoicesClosed;

    public bool IsChoosing { get; private set; }
    public IReadOnlyList<SkillCandidate> CurrentChoices => _currentChoices;

    /// <summary>
    /// 選択した際の動き
    /// </summary>
    /// <param name="index">0始まりの選択肢番号</param>
    /// <returns></returns>
    public bool SelectChoice(int index)
    {
        if (!IsChoosing || _applyingChoice || index < 0 || index >= _currentChoices.Count ||
            _player == null || !_player.Model.IsAlive)
        {
            return false;
        }

        SkillCandidate selected = _currentChoices[index];
        _applyingChoice = true;
        try
        {
            if (!selected.TryApply(_context))
            {
                if (IsChoosing)
                    RefreshChoices();
                return false;
            }

            CloseSelection();
            SkillSelected?.Invoke(selected.Skill);
        }
        finally
        {
            _applyingChoice = false;
        }

        OpenNextSelection();
        return true;
    }

    /// <summary>
    /// セレクトをキャンセルする処理
    /// </summary>
    public void CancelSelection()
    {
        _pendingSelections = 0;
        CloseSelection();
    }

    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private GameSessionManager _session;
    [SerializeField] private List<SkillBase> _skillPool = new();

    [Header("シーンのCanvas上にある選択UI")]
    [SerializeField] private SkillSelectionUI _selectionUI;

    private readonly List<SkillCandidate> _currentChoices = new();
    private readonly List<SkillCandidate> _availableCandidates = new();
    private readonly List<SkillCandidate> _generatedCandidates = new();
    private readonly HashSet<string> _candidateKeys = new();
    private SkillContext _context;
    private PlayerInputReader _playerInput;
    private int _pendingSelections;
    private bool _initialized;
    private bool _applyingChoice;

    private void Start()
    {
        if (_session == null)
        {
            Debug.LogError("SkillSelectionManagerにシーン上のGameSessionManagerを設定してください。", this);
            enabled = false;
            return;
        }

        if (_selectionUI == null)
        {
            Debug.LogError("SkillSelectionManagerにシーン上のSkillSelectionUIを設定してください。", this);
            enabled = false;
            return;
        }

        // 画面はシーンのCanvasに配置し、ここでは既存UIと接続する。
        _initialized = _selectionUI.Initialize(this);
        if (!_initialized)
        {
            enabled = false;
            return;
        }

        OpenNextSelection();
    }

    private void Awake()
    {
        _context = new SkillContext(_player, _weaponManager);
        if (_player != null)
        {
            _playerInput = _player.GetComponent<PlayerInputReader>();
        }
    }

    private void OnEnable()
    {
        if (_player != null)
        {
            _player.Model.LeveledUp += OnPlayerLeveledUp;
            _player.PlayerDied += CancelSelection;
        }
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.Model.LeveledUp -= OnPlayerLeveledUp;
            _player.PlayerDied -= CancelSelection;
        }

        CancelSelection();
    }

    /// <summary>
    /// playerのレベルUP処理
    /// </summary>
    /// <param name="levelsGained"></param>
    private void OnPlayerLeveledUp(int levelsGained)
    {
        _pendingSelections += levelsGained;
        OpenNextSelection();
    }

    private void Update()
    {
        // 外部の一時停止中に受けたレベルアップも、再開時に処理する。
        if (_pendingSelections > 0 && !IsChoosing)
            OpenNextSelection();
    }

    /// <summary>
    /// 選択肢を表示させる処理
    /// </summary>
    private void OpenNextSelection()
    {
        if (!_initialized || _pendingSelections <= 0 || !isActiveAndEnabled || IsChoosing || _applyingChoice ||
            _player == null || !_player.Model.IsAlive || _session == null || !_session.TryPauseForSkillSelection())
        {
            return;
        }

        BuildChoices();
        if (_currentChoices.Count == 0)
        {
            _session.ResumeAfterSkillSelection();
            ClearUnavailableSelections();
            return;
        }

        _pendingSelections--;
        IsChoosing = true;
        _playerInput?.ClearJumpRequest();
        ChoicesOpened?.Invoke();
    }

    /// <summary>
    /// カードの候補を生成する
    /// </summary>
    private void BuildChoices()
    {
        _currentChoices.Clear();
        _availableCandidates.Clear();
        _candidateKeys.Clear();

        foreach (SkillBase skill in _skillPool)
        {
            if (skill == null)
            {
                continue;
            }

            _generatedCandidates.Clear();
            skill.BuildCandidates(_context, _generatedCandidates);
            foreach (SkillCandidate candidate in _generatedCandidates)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.IdentityKey) ||
                    !_candidateKeys.Add(candidate.IdentityKey))
                {
                    continue;
                }

                _availableCandidates.Add(candidate);
            }
        }

        for (int i = 0; i < ChoiceCount && _availableCandidates.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, _availableCandidates.Count);
            SkillCandidate selected = _availableCandidates[randomIndex];
            _currentChoices.Add(selected);

            if (!selected.AllowDuplicateInSameSelection)
            {
                _availableCandidates.RemoveAt(randomIndex);
            }
        }

        _generatedCandidates.Clear();
        _availableCandidates.Clear();
    }

    /// <summary>
    /// カード選択やり直し
    /// </summary>
    private void RefreshChoices()
    {
        BuildChoices();
        if (_currentChoices.Count > 0)
        {
            ChoicesChanged?.Invoke();
            return;
        }

        ClearUnavailableSelections();
        CloseSelection();
    }

    private void ClearUnavailableSelections()
    {
        _pendingSelections = 0;
        Debug.LogWarning("適用可能なスキルがありません。", this);
    }

    /// <summary>
    /// セレクション画面を閉じたときの処理
    /// </summary>
    private void CloseSelection()
    {
        if (!IsChoosing)
        {
            return;
        }

        IsChoosing = false;
        _currentChoices.Clear();
        _playerInput?.ClearJumpRequest();
        if (_session != null)
        {
            _session.ResumeAfterSkillSelection();
        }
        ChoicesClosed?.Invoke();
    }
}
