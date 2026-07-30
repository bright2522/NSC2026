using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNameEntryUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text hintText;

    private Action<string> _onConfirm;
    private bool _isVisible;
    private bool _wired;

    private void Awake()
    {
        EnsureWired();
    }

    private void OnEnable()
    {
        EnsureWired();
    }

    private void OnDisable()
    {
        if (root != null)
        {
            LobbyUIAnimations.Cancel(root);
        }
    }

    private void EnsureWired()
    {
        if (_wired)
        {
            return;
        }

        _wired = true;

        if (root == null)
        {
            root = gameObject;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirm);
            confirmButton.onClick.AddListener(HandleConfirm);
            LobbyUIAnimations.SetupButtonFeedback(confirmButton);
        }

        if (nameInput != null)
        {
            nameInput.onSubmit.RemoveAllListeners();
            nameInput.onSubmit.AddListener(_ => HandleConfirm());
            nameInput.characterLimit = 16;
        }
    }

    public void Show(Action<string> onConfirm)
    {
        EnsureWired();

        _onConfirm = onConfirm;
        _isVisible = true;

        if (titleText != null)
        {
            titleText.text = "Enter Your Name";
        }

        if (hintText != null)
        {
            hintText.text = "This name will be visible to the host.";
        }

        if (nameInput != null)
        {
            nameInput.text = string.Empty;
        }

        if (root == null)
        {
            root = gameObject;
        }

        LobbyUIAnimations.AnimatePanelIn(root, 0.05f, () =>
        {
            if (nameInput != null)
            {
                nameInput.Select();
                nameInput.ActivateInputField();
            }
        });
    }

    public void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        _isVisible = false;
        _onConfirm = null;

        if (root == null)
        {
            return;
        }

        LobbyUIAnimations.AnimatePanelOut(root);
    }

    private void HandleConfirm()
    {
        string name = nameInput != null ? nameInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            if (nameInput != null)
            {
                LobbyUIAnimations.AnimatePopText(nameInput.placeholder as TMP_Text ?? nameInput.textComponent);
            }

            return;
        }

        var callback = _onConfirm;
        Hide();
        callback?.Invoke(name);
    }
}
