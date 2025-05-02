using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public Image[] hearts; // Массив изображений сердец

    [Header("Debug")]
    public Button increaseHealthButton;
    public Button decreaseHealthButton;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        // Настройка отладочных кнопок
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
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHealth;
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        // Здесь можно добавить логику смерти
    }
}