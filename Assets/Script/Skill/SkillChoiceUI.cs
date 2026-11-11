using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択肢
/// </summary>
public sealed class SkillChoiceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text categoryText;
    [SerializeField] private Image iconImage;

    private int choiceIndex;
    private Action<int> onSelected;

    public Button Button => button;
    public bool IsConfigured => button != null && nameText != null && descriptionText != null;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(Select);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(Select);
        }
    }

    /// <summary>
    /// カテゴリーによって名前を変える
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="index"></param>
    /// <param name="selectionHandler"></param>
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
                _ => "武器強化"
            };
        }
        if (iconImage != null)
        {
            iconImage.sprite = candidate.Icon;
            iconImage.enabled = candidate.Icon != null;
        }
    }

    private void Select()
    {
        onSelected?.Invoke(choiceIndex);
    }
}
