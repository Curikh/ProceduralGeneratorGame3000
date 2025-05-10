using UnityEngine;
using System.Collections;
using System.Collections.Generic; //  Нужен для List<>

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f;
    public float returnSpeedMultiplier = 1.5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Detection Zone")]
    public float detectionRadius = 5f;
    public bool showGizmos = true;

    [Header("Anchor System")]
    public Vector2 anchorPosition;
    public bool useTransformAsAnchor = true;

    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 0.5f;

    [Header("Drop Settings")]
    public GameObject[] itemDrops; //  Массив префабов предметов для дропа
    [Range(0f, 1f)] public float dropChance = 0.5f; //  Шанс выпадения предмета (от 0 до 1)
    public float dropForce = 3f; //  Сила, с которой предметы разбрасываются

    [Header("Animation Settings")]
    public Animator animator;
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string isMovingParam = "IsMoving";

    [Header("Damage Effects")]
    public float hitFlashDuration = 0.2f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Vector2 lastMoveDirection;
    private Transform player;
    private bool isChasing = false;
    private CircleCollider2D detectionCollider;
    private PlayerHealth playerHealth;

    [Header("Knockback Settings")]
public float knockbackResistance = 1f; // 1 = нормальное отталкивание, 0.5 = слабее, 2 = сильнее
private bool isKnockedBack = false;
private Vector2 knockbackVelocity;
private float knockbackDuration = 0.2f;
private float knockbackTimer = 0f;

    private void Awake()
    {
        InitializeDetectionZone();
        if (useTransformAsAnchor)
        {
            anchorPosition = transform.position;
        }
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void InitializeDetectionZone()
    {
        detectionCollider = GetComponentInChildren<CircleCollider2D>();
        if (detectionCollider == null)
        {
            GameObject zone = new GameObject("DetectionZone");
            zone.transform.SetParent(transform);
            zone.transform.localPosition = Vector3.zero;

            detectionCollider = zone.AddComponent<CircleCollider2D>();
            detectionCollider.isTrigger = true;
        }

        detectionCollider.radius = detectionRadius;
    }

            public void ApplyKnockback(Vector2 direction, float force)
        {
            if (isKnockedBack) return;
            
            knockbackVelocity = direction * force / knockbackResistance;
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
        }

    private void Update()
{
    if (isKnockedBack)
    {
        knockbackTimer -= Time.deltaTime;
        transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
        
        if (knockbackTimer <= 0)
        {
            isKnockedBack = false;
        }
    }
    else
    {
        // Обычное поведение
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            ReturnToAnchor();
        }
    }
    
    UpdateAnimation();
}

    private void ChasePlayer()
    {
      if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        lastMoveDirection = direction;

        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }
    private IEnumerator HitFlashEffect()
{
    spriteRenderer.color = Color.red;
    yield return new WaitForSeconds(hitFlashDuration);
    spriteRenderer.color = originalColor;
}

    private void AttackPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.DecreaseHealth();
            Debug.Log("Enemy attacked player!");
        }
    }

    private void ReturnToAnchor()
    {
        Vector2 direction = (anchorPosition - (Vector2)transform.position).normalized;
        lastMoveDirection = direction;

        if (Vector2.Distance(transform.position, anchorPosition) > 0.1f)
        {
            float speed = moveSpeed * returnSpeedMultiplier;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }

    public void SetNewAnchorPosition(Vector2 newPosition)
    {
        anchorPosition = newPosition;
    }

    public void ResetAnchorToCurrentPosition()
    {
        anchorPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy received {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitFlashEffect());
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        DropItem(); //  Вызываем метод дропа
        Destroy(gameObject);
    }

        private void UpdateAnimation()
    {
        if (animator == null) return;

        // Нормализуем направление, чтобы избежать перекоса при диагональном движении
        Vector2 normalizedDirection = lastMoveDirection.normalized;

        animator.SetFloat(moveXParam, normalizedDirection.x);
        animator.SetFloat(moveYParam, normalizedDirection.y);

        // Проверяем, движется ли враг (используем для перехода между Idle и Move)
        bool isMoving = (isChasing && player != null) || 
                       (Vector2.Distance(transform.position, anchorPosition) > 0.1f);
        animator.SetBool(isMovingParam, isMoving);
    }

    private void DropItem()
    {
        if (itemDrops.Length == 0) return; //  Если массив пустой, выходим

        float randomValue = Random.value; //  Случайное значение от 0 до 1

        if (randomValue <= dropChance)
        {
            //  Выбираем случайный предмет из массива
            int randomIndex = Random.Range(0, itemDrops.Length);
            GameObject droppedItem = Instantiate(itemDrops[randomIndex], transform.position, Quaternion.identity);

            //  Применяем силу, чтобы разбросать предмет (необязательно)
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * dropForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isChasing = true;
            playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealth component not found on player!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            player = null;
            playerHealth = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(useTransformAsAnchor ? transform.position : anchorPosition, 0.2f);
        Gizmos.DrawLine(transform.position, useTransformAsAnchor ? transform.position : anchorPosition);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}