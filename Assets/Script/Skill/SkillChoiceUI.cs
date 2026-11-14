using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>選択肢プレハブの表示を、抽選された強化候補に合わせる。</summary>
public sealed class SkillChoiceUI : MonoBehaviour
{
    private const int StarSlotCount = 5;

    [Serializable]
    private struct CardFrame
    {
        public SkillCategory category;
        public Sprite sprite;
        public Color tint;
    }

    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private Image iconImage;

    [Header("カテゴリ別のカード外枠")]
    [SerializeField] private Image frameImage;
    [SerializeField] private CardFrame[] cardFrames;

    [Header("武器の取得・強化表示")]
    [SerializeField] private Image newWeaponIndicator;
    [SerializeField] private Sprite filledStar;
    [SerializeField] private Sprite offeredStar;
    [SerializeField] private Image[] starSlots = new Image[StarSlotCount];

    private int choiceIndex;
    private Action<int> onSelected;

    public Button Button => button;
    public bool IsConfigured => button != null && nameText != null && descriptionText != null;

    /// <summary>再利用するカードに候補のテキスト・画像・クリック先を反映する。</summary>
    public void Bind(SkillCandidate candidate, int index, Action<int> selectionHandler)
    {
        choiceIndex = index;
        onSelected = selectionHandler;
        nameText.text = candidate.DisplayName;
        descriptionText.text = candidate.Description;

        if (categoryText != null)
        {
            categoryText.text = candidate.Category switch
            {
                SkillCategory.Weapon => "武器",
                SkillCategory.SelfEnhancement => "自己強化",
                SkillCategory.WeaponRarityUpgrade => "武器強化",
                _ => string.Empty
            };
        }
        if (iconImage != null)
        {
            iconImage.sprite = candidate.Icon;
            iconImage.enabled = candidate.Icon != null;
        }

        ApplyFrame(candidate.Category);
        ApplyWeaponIndicators(candidate);
    }

    /// <summary>プレハブのボタンを選択処理へ接続する。</summary>
    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(Select);
        }
    }

    /// <summary>プレハブを破棄するときにボタンとの接続を解除する。</summary>
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(Select);
        }
    }

    /// <summary>カテゴリに対応する外枠を設定し、未登録の場合は画像と色を初期化する。</summary>
    private void ApplyFrame(SkillCategory category)
    {
        if (frameImage == null || cardFrames == null)
        {
            return;
        }

        foreach (CardFrame frame in cardFrames)
        {
            if (frame.category == category)
            {
                frameImage.sprite = frame.sprite;
                frameImage.color = frame.tint;
                return;
            }
        }

        frameImage.sprite = null;
        frameImage.color = Color.white;
    }

    /// <summary>武器獲得時はNEW、武器強化時は現在の星と取得予定の星を表示する。</summary>
    private void ApplyWeaponIndicators(SkillCandidate candidate)
    {
        if (newWeaponIndicator != null)
        {
            newWeaponIndicator.enabled = candidate.Category == SkillCategory.Weapon;
        }

        bool showStars = candidate.Category == SkillCategory.WeaponRarityUpgrade;
        for (int i = 0; i < Mathf.Min(StarSlotCount, starSlots.Length); i++)
        {
            Image slot = starSlots[i];
            if (slot == null)
            {
                continue;
            }

            int level = i + 1;
            Sprite sprite = null;
            if (showStars && level <= candidate.CurrentStarLevel)
            {
                sprite = filledStar;
            }
            else if (showStars && level == candidate.OfferedStarLevel)
            {
                sprite = offeredStar;
            }

            slot.sprite = sprite;
            slot.enabled = sprite != null;
        }
    }

    /// <summary>このカードに割り当てられた選択肢番号を通知する。</summary>
    private void Select()
    {
        onSelected?.Invoke(choiceIndex);
    }
}
