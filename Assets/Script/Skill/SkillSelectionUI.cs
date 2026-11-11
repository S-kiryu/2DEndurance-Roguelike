using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>シーンのCanvas上で画面を管理し、選択肢プレハブだけを生成する。</summary>
public sealed class SkillSelectionUI : MonoBehaviour
{
    [Header("シーンのCanvas内の表示先")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform choicesRoot;

    [Header("生成する選択肢プレハブ")]
    [SerializeField] private SkillChoiceUI choicePrefab;

    private readonly List<SkillChoiceUI> choices = new();
    private SkillSelectionManager selection;

    private void Awake()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    public bool Initialize(SkillSelectionManager manager)
    {
        if (manager == null || selectionPanel == null || selectionPanel == gameObject ||
            choicesRoot == null || choicePrefab == null || !choicePrefab.IsConfigured)
        {
            Debug.LogError("SkillSelectionUIにシーンのパネル・生成先・Choice1プレハブを設定してください。", this);
            return false;
        }

        selection = manager;
        selection.ChoicesOpened += Show;
        selection.ChoicesClosed += Hide;
        if (selection.IsChoosing)
        {
            Show();
        }
        return true;
    }

    private void OnDestroy()
    {
        if (selection != null)
        {
            selection.ChoicesOpened -= Show;
            selection.ChoicesClosed -= Hide;
        }
    }

    /// <summary>クリックされた選択肢から、0始まりの番号を受け取る。</summary>
    public void SelectChoice(int index)
    {
        if (selection != null && selection.isActiveAndEnabled)
        {
            selection.SelectChoice(index);
        }
    }

    private void Show()
    {
        // シーンにある生成先へ、候補数に応じてプレハブを生成
        while (choices.Count < selection.CurrentChoices.Count)
        {
            choices.Add(Instantiate(choicePrefab, choicesRoot));
        }

        for (int i = 0; i < choices.Count; i++)
        {
            SkillChoiceUI view = choices[i];
            bool available = i < selection.CurrentChoices.Count;
            if (available)
            {
                view.Bind(selection.CurrentChoices[i], i, SelectChoice);
            }
            view.gameObject.SetActive(available);
        }

        selectionPanel.SetActive(true);
    }

    private void Hide()
    {
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(selectionPanel.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        selectionPanel.SetActive(false);
    }
}
