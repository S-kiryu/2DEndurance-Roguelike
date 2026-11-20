using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>選択肢プレハブの表示を、抽選された強化候補に合わせる。</summary>
public sealed class SkillChoiceUI : MonoBehaviour
{
    public Button Button => _button;
    public bool IsConfigured => _button != null && _nameText != null;
    public string Description { get; private set; }

    /// <summary>再利用するカードに候補のテキスト・画像・クリック先を反映する。</summary>
    public void Bind(SkillCandidate candidate, int index, Action<int> selectionHandler)
    {
        _choiceIndex = index;
        _onSelected = selectionHandler;
        _nameText.text = candidate.DisplayName;
        Description = candidate.Description;

        if (_categoryText != null)
        {
            _categoryText.text = candidate.Category switch
            {
                SkillCategory.Weapon => "武器",
                SkillCategory.SelfEnhancement => "自己強化",
                SkillCategory.WeaponRarityUpgrade => "武器強化",
                _ => string.Empty
            };
        }
        if (_iconImage != null)
        {
            _iconImage.sprite = candidate.Icon;
            _iconImage.enabled = candidate.Icon != null;
        }

        ApplyFrame(candidate.Category);
        ApplyWeaponIndicators(candidate);
    }

    private const int _starSlotCount = 5;

    [Serializable]
    private struct CardFrame
    {
        public SkillCategory category;
        public Sprite sprite;
        public Color tint;
    }

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _categoryText;
    [SerializeField] private Image _iconImage;

    [Header("カテゴリ別のカード外枠")]
    [SerializeField] private Image _frameImage;
    [SerializeField] private CardFrame[] _cardFrames;

    [Header("武器の取得・強化表示")]
    [SerializeField] private Image _newWeaponIndicator;
    [SerializeField] private Sprite _filledStar;
    [SerializeField] private Sprite _offeredStar;
    [SerializeField] private Image[] _starSlots = new Image[_starSlotCount];

    private int _choiceIndex;
    private Action<int> _onSelected;

    /// <summary>プレハブのボタンを選択処理へ接続する。</summary>
    private void Awake()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(Select);
        }
    }

    /// <summary>プレハブを破棄するときにボタンとの接続を解除する。</summary>
    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(Select);
        }
    }

    /// <summary>カテゴリに対応する外枠を設定し、未登録の場合は画像と色を初期化する。</summary>
    private void ApplyFrame(SkillCategory category)
    {
        if (_frameImage == null || _cardFrames == null)
        {
            return;
        }

        foreach (CardFrame frame in _cardFrames)
        {
            if (frame.category == category)
            {
                _frameImage.sprite = frame.sprite;
                _frameImage.color = frame.tint;
                return;
            }
        }

        _frameImage.sprite = null;
        _frameImage.color = Color.white;
    }

    /// <summary>武器獲得時はNEW、武器強化時は現在の星と取得予定の星を表示する。</summary>
    private void ApplyWeaponIndicators(SkillCandidate candidate)
    {
        if (_newWeaponIndicator != null)
        {
            _newWeaponIndicator.overrideSprite = (candidate.Skill as WeaponAcquireSkill)?.NewWeaponIndicator;
            _newWeaponIndicator.enabled = candidate.Category == SkillCategory.Weapon;
        }

        bool showStars = candidate.Category == SkillCategory.WeaponRarityUpgrade;
        for (int i = 0; i < Mathf.Min(_starSlotCount, _starSlots.Length); i++)
        {
            Image slot = _starSlots[i];
            if (slot == null)
            {
                continue;
            }

            int level = i + 1;
            Sprite sprite = null;
            if (showStars && level <= candidate.CurrentStarLevel)
            {
                sprite = _filledStar;
            }
            else if (showStars && level == candidate.OfferedStarLevel)
            {
                sprite = _offeredStar;
            }

            slot.sprite = sprite;
            slot.enabled = sprite != null;
        }
    }

    /// <summary>このカードに割り当てられた選択肢番号を通知する。</summary>
    private void Select()
    {
        _onSelected?.Invoke(_choiceIndex);
    }
}
