using UnityEngine;
using UnityEditor;

namespace Inventory
{
    public class PlayerAttack : MonoBehaviour
    {
        public GameObject attackEffectPrefab;
        private PlayerMovement playerMovement;
        private InventoryManager inventoryManager;

        private Vector2 lastMoveDirection;
        private float attackCooldownTimer = 0f;
        private float currentAttackRange;
        private GameObject currentAttackEffect; //  Добавили переменную для хранения эффекта

        void Start()
        {
            playerMovement = GetComponent<PlayerMovement>();
            inventoryManager = GetComponent<InventoryManager>();
            lastMoveDirection = Vector2.down;
        }

        void Update()
        {
            if (playerMovement.movement.magnitude > 0.1f)
            {
                lastMoveDirection = playerMovement.movement.normalized;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Attack();
            }

            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
            }
        }

        void Attack()
        {
            Item currentWeapon = inventoryManager.GetCurrentWeapon();
            if (currentWeapon == null || attackCooldownTimer > 0) return;

            currentAttackRange = currentWeapon.attackRange;
            Vector2 attackDirection = lastMoveDirection;
            Vector2 attackPosition = transform.position + (Vector3)attackDirection * currentAttackRange;

            if (attackEffectPrefab != null)
            {
                currentAttackEffect = Instantiate(attackEffectPrefab, attackPosition, Quaternion.identity); //  Сохраняем ссылку на эффект
                currentAttackEffect.transform.SetParent(transform); //  Делаем эффект дочерним

                Animator effectAnimator = currentAttackEffect.GetComponent<Animator>();
                effectAnimator.SetFloat("DirectionX", attackDirection.x);
                effectAnimator.SetFloat("DirectionY", attackDirection.y);

                Destroy(currentAttackEffect, 0.5f); //  Уничтожаем дочерний эффект через 0.5 сек.
            }

            //  Логика нанесения урона
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, currentAttackRange);
            foreach (Collider2D hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentWeapon.damage);
                }
            }

            Debug.Log("Атака в направлении: " + attackDirection + " с дальностью: " + currentAttackRange);
            attackCooldownTimer = currentWeapon.attackCooldown;
        }

        void OnDrawGizmosSelected()
        {
            if (inventoryManager == null) return;
            Item currentWeapon = inventoryManager.GetCurrentWeapon();
            if (currentWeapon == null) return;

            currentAttackRange = currentWeapon.attackRange;
            Vector2 attackDirection = lastMoveDirection;
            Vector2 attackPosition = transform.position + (Vector3)attackDirection * currentAttackRange;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPosition, currentWeapon.attackRange);
        }
    }
}