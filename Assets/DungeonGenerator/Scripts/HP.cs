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
    private PlayerMovement playerMovement;
    private Collider2D playerCollider;
    public Animator animator;
    public bool isDead;

    public void Start()
    {
         currentHealth = maxHealth;
         isDead = false;
         UpdateHearts();

        playerMovement = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<Collider2D>();

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
        if (isDead) return;
        isDead = true;
        Debug.Log("Player died!");
        playerMovement.enabled = false;
        playerCollider.enabled = false;
        animator.SetTrigger("Die"); 
        // Здесь можно добавить логику смерти
    }
}