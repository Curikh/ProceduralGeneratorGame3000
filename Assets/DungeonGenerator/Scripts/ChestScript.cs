using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Inventory;

public class ChestScript : MonoBehaviour
{
    [Header("Loot Generation")]
    [SerializeField] private GameObject[] LootToChooseFrom;
    [SerializeField] private int ItemsToGenerateMin;
    [SerializeField] private int ItemsToGenerateMax;
    [SerializeField] private int StackableItemsToGenerateMin;
    [SerializeField] private int StackableItemsToGenerateMax;
    [SerializeField] private float ItemsGenerationSpread;

    [Header("Interaction Settings")]
    [SerializeField] private float openRadius = 2.0f; // Радиус взаимодействия
    [SerializeField] private bool isOpened = false; // Флаг открытия
	[SerializeField] private Sprite closedSprite; // Спрайт закрытого сундука
    [SerializeField] private Sprite openSprite;

    public UnityEvent onOpenEvent = new UnityEvent(); // Событие при открытии

    public enum DropDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public DropDirection dropDirection = DropDirection.Up;
	private SpriteRenderer spriteRenderer;
    private List<GameObject> ChosenLoot = new List<GameObject>();
    private Transform playerTransform; // Для проверки расстояния
	private PlayerKeys playerKeys;

    void Start()
    {
        // Находим игрока по тегу (убедитесь, что у игрока есть тег "Player")
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
		playerKeys = playerTransform.GetComponent<PlayerKeys>();
		spriteRenderer = GetComponent<SpriteRenderer>();

        // Генерация лута
        int additionalItemsCount = Random.Range(0, ItemsToGenerateMax - ItemsToGenerateMin);
        for (int i = 0; i < (ItemsToGenerateMin + additionalItemsCount); i++)
            ChosenLoot.Add(LootToChooseFrom[Random.Range(0, LootToChooseFrom.Length)]);
    }

    void OnMouseDown()
    {
        if (isOpened) return; // Если сундук уже открыт, ничего не делаем

        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > openRadius) return; // Игрок слишком далеко
		if (playerKeys == null || playerKeys.currentKeys <= 0)
            {
                Debug.Log("Недостаточно ключей!");
                return;
            }
        OpenChest();
		playerKeys.RemoveKey();
    }


    private void OpenChest()
    {
        isOpened = true;
		spriteRenderer.sprite = openSprite;

        // Генерация лута
        int additionalStackableItemRange = Mathf.Abs(StackableItemsToGenerateMax - StackableItemsToGenerateMin);
        foreach (GameObject loot in ChosenLoot)
        {
            GameObject newLoot = Instantiate(loot, this.transform.position, Quaternion.identity);
            bool isLootStackable = newLoot.GetComponent<WorldItem>().item.stackable;
            int amountToSpawn = 1;
            if (isLootStackable) amountToSpawn = StackableItemsToGenerateMin + Random.Range(0, additionalStackableItemRange);
            
            for (int i = 0; i < amountToSpawn; i++)
            {
                Vector3 newPosition = GenerateSpread(newLoot.transform.position);
                newLoot.transform.position = newPosition;
            }
        }

        // Вызываем событие открытия
        onOpenEvent.Invoke();

        // Уничтожаем сундук (если нужно)
       // Destroy(gameObject, 1.0f);Задержка для анимации
    }

    private Vector3 GenerateSpread(Vector3 originalPos)
    {
        Vector3 newSpread = new Vector3();
        float newSpreadX;
        float newSpreadY;
        switch (dropDirection)
        {
            case DropDirection.Up:
                newSpreadX = originalPos.x - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
                newSpreadY = originalPos.y + (Random.value * ItemsGenerationSpread) / 2;
                newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
                break;
            case DropDirection.Right:
                newSpreadX = originalPos.x + (Random.value * ItemsGenerationSpread) / 2;
                newSpreadY = originalPos.y - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
                newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
                break;
            case DropDirection.Down:
                newSpreadX = originalPos.x - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
                newSpreadY = originalPos.y - (Random.value * ItemsGenerationSpread) / 2;
                newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
                break;
            case DropDirection.Left:
                newSpreadX = originalPos.x - (Random.value * ItemsGenerationSpread) / 2;
                newSpreadY = originalPos.y - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
                newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
                break;
        }
        return newSpread;
    }

    // Визуализация радиуса взаимодействия в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, openRadius);
    }



}

