using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>シーンのCanvas上で画面を管理し、選択肢プレハブだけを生成する。</summary>
public sealed class SkillSelectionUI : MonoBehaviour
{
    public bool Initialize(SkillSelectionManager manager)
    {
        if (manager == null || _selectionPanel == null || _selectionPanel == gameObject ||
            _choicesRoot == null || _descriptionPanel == null || _descriptionText == null ||
            _choicePrefab == null || !_choicePrefab.IsConfigured)
        {
            Debug.LogError("SkillSelectionUIにシーンのパネル・生成先・説明欄・Choice1プレハブを設定してください。", this);
            return false;
        }

        SetDescription(null);
        _selection = manager;
        _selection.ChoicesOpened += Show;
        _selection.ChoicesChanged += Show;
        _selection.ChoicesClosed += Hide;
        if (_selection.IsChoosing)
        {
            Show();
        }
        return true;
    }

    /// <summary>クリックされた選択肢から、0始まりの番号を受け取る。</summary>
    public void SelectChoice(int index)
    {
        if (_selection != null && _selection.isActiveAndEnabled)
        {
            _selection.SelectChoice(index);
        }
    }

    [Header("シーンのCanvas内の表示先")]
    [SerializeField,Tooltip("選択肢が表示されるパネル")] private GameObject _selectionPanel;
    [SerializeField,Tooltip("選択肢が生成される親オブジェクト")] private Transform _choicesRoot;
    [SerializeField] private GameObject _descriptionPanel;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("生成する選択肢プレハブ")]
    [SerializeField] private SkillChoiceUI _choicePrefab;

    private readonly List<SkillChoiceUI> _choices = new();
    private SkillSelectionManager _selection;

    private void Awake()
    {
        if (_selectionPanel != null)
        {
            _selectionPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_selection != null)
        {
            _selection.ChoicesOpened -= Show;
            _selection.ChoicesChanged -= Show;
            _selection.ChoicesClosed -= Hide;
        }
    }

    private void LateUpdate()
    {
        if (_selection == null || !_selection.IsChoosing || !_selectionPanel.activeInHierarchy)
        {
            return;
        }

        string description = null;
        Mouse mouse = Mouse.current;
        if (mouse != null && EventSystem.current != null &&
            EventSystem.current.currentInputModule is InputSystemUIInputModule inputModule)
        {
            // EventSystemがマウスの判定に使った結果を再利用する。
            GameObject hit = inputModule.GetLastRaycastResult(mouse.deviceId).gameObject;
            SkillChoiceUI choice = hit != null ? hit.GetComponentInParent<SkillChoiceUI>() : null;
            if (choice != null && choice.isActiveAndEnabled && choice.transform.parent == _choicesRoot)
            {
                description = choice.Description;
            }
        }

        SetDescription(description);
    }

    /// <summary>
    /// 説明を作る
    /// </summary>
    /// <param name="description"></param>
    private void SetDescription(string description)
    {
        _descriptionText.text = description ?? string.Empty;
        bool visible = !string.IsNullOrWhiteSpace(description);
        if (_descriptionPanel.activeSelf != visible)
        {
            _descriptionPanel.SetActive(visible);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Show()
    {
        // シーンにある生成先へ、候補数に応じてプレハブを生成
        while (_choices.Count < _selection.CurrentChoices.Count)
        {
            _choices.Add(Instantiate(_choicePrefab, _choicesRoot));
        }

        for (int i = 0; i < _choices.Count; i++)
        {
            SkillChoiceUI view = _choices[i];
            bool available = i < _selection.CurrentChoices.Count;
            if (available)
            {
                view.Bind(_selection.CurrentChoices[i], i, SelectChoice);
            }
            view.gameObject.SetActive(available);
        }

        _selectionPanel.SetActive(true);
    }

    /// <summary>
    /// 閉じる処理
    /// </summary>
    private void Hide()
    {
        SetDescription(null);
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(_selectionPanel.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        _selectionPanel.SetActive(false);
    }
}
