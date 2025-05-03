using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public Image[] heartImages; // Массив компонентов Image для сердец

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Debug")]
    public Button increaseHealthButton;
    public Button decreaseHealthButton;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        // Настройка кнопок отладки
        if (increaseHealthButton != null)
            increaseHealthButton.onClick.AddListener(IncreaseHealth);
        
        if (decreaseHealthButton != null)
            decreaseHealthButton.onClick.AddListener(DecreaseHealth);
    }

    public void IncreaseHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHearts();
        }
    }

    public void DecreaseHealth()
    {
        if (currentHealth > 0)
        {
            currentHealth--;
            UpdateHearts();
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // Показываем все Image, но меняем спрайты
            heartImages[i].enabled = true;
            heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        // Здесь можно добавить логику смерти
    }
}