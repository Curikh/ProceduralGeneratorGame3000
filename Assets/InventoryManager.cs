using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int maxStackedItems = 10;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;

    private void Start() {
        ChangeSelectedSlot(0); // Выбираем первый слот по умолчанию
    }
    private void Update(){
        if (Input.inputString != null) {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number < 5) {
                ChangeSelectedSlot(number-1);
            }
        }
    }
    void ChangeSelectedSlot(int newValue) {
        if (selectedSlot >= 0) {
            inventorySlots[selectedSlot].Deselect(); // Снимаем выделение
        }
        
        inventorySlots[newValue].Select(); // Выделяем новый слот
        selectedSlot = newValue;
    }

    public bool AddItem(Item item) {
        // Проверка на стекирование
        for (int i = 0; i < inventorySlots.Length; i++) {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == item && 
                itemInSlot.count < maxStackedItems && itemInSlot.item.stackable) {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }
        
        // Проверка на пустые слоты
        for (int i = 0; i < inventorySlots.Length; i++) {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null) {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, InventorySlot slot) {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }
}