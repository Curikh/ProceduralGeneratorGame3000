using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f;
    
    [Header("Detection Zone")]
    public float detectionRadius = 5f;
    public Transform detectionZone;
    
    private Transform player;
    private Vector3 homePosition;
    private bool isChasing = false;

    private void Awake()
    {
        homePosition = transform.position;
        InitializeDetectionZone();
    }

    private void InitializeDetectionZone()
    {
        if (detectionZone == null)
        {
            GameObject zone = new GameObject("DetectionZone");
            zone.transform.SetParent(transform);
            zone.transform.localPosition = Vector3.zero;
            
            var collider = zone.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = detectionRadius;
            
            detectionZone = zone.transform;
        }
    }

    private void Update()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            ReturnHome();
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

    private void ReturnHome()
    {
        if (Vector2.Distance(transform.position, homePosition) > 0.1f)
        {
            Vector2 direction = (homePosition - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
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

    // Для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}