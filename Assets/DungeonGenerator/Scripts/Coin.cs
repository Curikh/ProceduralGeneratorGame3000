using UnityEngine;
using UnityEngine.UI;

public class PlayerCoins : MonoBehaviour
{
    [Header("Coin Settings")]
    public int maxCoins = 50;
    public int currentCoins;
    public Text coinsText;

    [Header("Debug")]
    public Button addCoinButton;
    public Button removeCoinButton;

    private void Start()
    {
        currentCoins = 0;
        UpdateCoinsDisplay();

        // Настройка отладочных кнопок
        if (addCoinButton != null)
            addCoinButton.onClick.AddListener(AddCoin);
        
        if (removeCoinButton != null)
            removeCoinButton.onClick.AddListener(RemoveCoin);
    }

    public void AddCoin()
    {
        if (currentCoins < maxCoins)
        {
            currentCoins++;
            UpdateCoinsDisplay();
        }
    }

    public void RemoveCoin()
    {
        if (currentCoins > 0)
        {
            currentCoins--;
            UpdateCoinsDisplay();
        }
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null)
        {
            coinsText.text = currentCoins.ToString();
        }
    }
}