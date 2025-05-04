using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f;
    public float returnSpeedMultiplier = 1.5f; // Быстрее возвращается на базу
    
    [Header("Detection Zone")]
    public float detectionRadius = 5f;
    public bool showGizmos = true;
    
    [Header("Anchor System")]
    [Tooltip("Точка, к которой возвращается противник")]
    public Vector2 anchorPosition;
    public bool useTransformAsAnchor = true; // Использовать текущую позицию как якорь
    
    private Transform player;
    private bool isChasing = false;
    private CircleCollider2D detectionCollider;

    private void Awake()
    {
        InitializeDetectionZone();
        if (useTransformAsAnchor)
        {
            anchorPosition = transform.position;
        }
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

    private void Update()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            ReturnToAnchor();
        }
    }

    private void ChasePlayer()
    {
        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
    }

    private void ReturnToAnchor()
    {
        if (Vector2.Distance(transform.position, anchorPosition) > 0.1f)
        {
            Vector2 direction = (anchorPosition - (Vector2)transform.position).normalized;
            float speed = moveSpeed * returnSpeedMultiplier; // Быстрее возвращается
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
    }

    // Метод для изменения якорной позиции
    public void SetNewAnchorPosition(Vector2 newPosition)
    {
        anchorPosition = newPosition;
        Debug.Log($"Новые координаты якоря: {anchorPosition}");
    }

    // Метод для сброса на текущую позицию
    public void ResetAnchorToCurrentPosition()
    {
        anchorPosition = transform.position;
        Debug.Log($"Якорь сброшен на текущую позицию: {anchorPosition}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isChasing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Визуализация зоны обнаружения
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Визуализация якорной точки
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(useTransformAsAnchor ? transform.position : anchorPosition, 0.2f);
        Gizmos.DrawLine(transform.position, useTransformAsAnchor ? transform.position : anchorPosition);
    }
}