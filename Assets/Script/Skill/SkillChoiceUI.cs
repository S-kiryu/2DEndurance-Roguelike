using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>1つの選択肢。プレハブ内の参照は複製先にも引き継がれる。</summary>
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

    public void Bind(SkillBase skill, int index, Action<int> selectionHandler)
    {
        choiceIndex = index;
        onSelected = selectionHandler;
        nameText.text = skill.DisplayName;
        descriptionText.text = skill.Description;

        if (categoryText != null)
        {
            categoryText.text = skill.Category switch
            {
                SkillCategory.Weapon => "武器",
                SkillCategory.SelfEnhancement => "自己強化",
                _ => "レアリティ強化（仮）"
            };
        }
        if (iconImage != null)
        {
            iconImage.sprite = skill.Icon;
            iconImage.enabled = skill.Icon != null;
        }
    }

    private void Select()
    {
        onSelected?.Invoke(choiceIndex);
    }
}
