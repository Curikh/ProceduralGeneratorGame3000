using UnityEngine;
using UnityEngine.UI;

public class PlayerKeys : MonoBehaviour
{
    [Header("Key Settings")]
    public int maxKeys = 50;
    public int currentKeys;
    public Text KeysText;

    [Header("Debug")]
    public Button addKeyButton;
    public Button removeKeyButton;

    private void Start()
    {
        currentKeys = 0;
        UpdateKeysDisplay();

        // Настройка отладочных кнопок
        if (addKeyButton != null)
            addKeyButton.onClick.AddListener(AddKey);
        
        if (removeKeyButton != null)
            removeKeyButton.onClick.AddListener(RemoveKey);
    }

    public void AddKey()
    {
        if (currentKeys < maxKeys)
        {
            currentKeys++;
            UpdateKeysDisplay();
        }
    }

    public void RemoveKey()
    {
        if (currentKeys > 0)
        {
            currentKeys--;
            UpdateKeysDisplay();
        }
    }

    void UpdateKeysDisplay()
    {
        if (KeysText != null)
        {
            KeysText.text = currentKeys.ToString();
        }
    }
}