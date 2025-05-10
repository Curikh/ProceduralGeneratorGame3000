using UnityEngine;

namespace Inventory 
{

	public class InventoryManager : MonoBehaviour
	{
		public int maxStackedItems = 10;
		public InventorySlot[] inventorySlots;
		public PlayerHealth playerHealth;
		public GameObject inventoryItemPrefab;
		public GameObject worldItemPrefab;
		public int selectedSlot = -1;

		private void Start() 
		{
			ChangeSelectedSlot(0); // Выбираем первый слот по умолчанию
		}
		private void Update()
		{
			if (Input.inputString != null) {
				bool isNumber = int.TryParse(Input.inputString, out int number);
				if (isNumber && number > 0 && number < 5) {
					ChangeSelectedSlot(number-1);
				}
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				UseSelectedItem();
			}


		}
		void ChangeSelectedSlot(int newValue) {
			if (selectedSlot >= 0) {
				inventorySlots[selectedSlot].Deselect(); // Снимаем выделение
			}

			inventorySlots[newValue].Select(); // Выделяем новый слот
			selectedSlot = newValue;
		}

		void UseSelectedItem()
		{
			if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length) return;

			InventorySlot slot = inventorySlots[selectedSlot];
			InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
			if (itemInSlot == null) return;

			if (itemInSlot.item.name == "HealingPoition")
			{
				if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
				{
					playerHealth.IncreaseHealth();

					itemInSlot.count--;
					if (itemInSlot.count <= 0)
					{
						Destroy(itemInSlot.gameObject);
					}
					else
					{
						itemInSlot.RefreshCount();
					}
				}
			}
		}


		public bool AddItem(Item item) 
		{
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
		public void DropItem(InventorySlot slot)
		{
			InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
			if (itemInSlot != null)
			{

				Vector3 dropPosition = transform.position + transform.forward * 1.5f;

				GameObject worldItem = Instantiate(worldItemPrefab, dropPosition, Quaternion.identity);

				WorldItem worldItemComponent = worldItem.GetComponent<WorldItem>();
				worldItemComponent.item = itemInSlot.item;
				worldItemComponent.amount = itemInSlot.count;

				Destroy(itemInSlot.gameObject); // Удаляем из UI
			}
		}
		public Item GetCurrentWeapon()
		{
			if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length) return null;

			InventorySlot slot = inventorySlots[selectedSlot];
			InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
			if (itemInSlot != null && itemInSlot.item.type == ItemType.Weapon)
			{
				return itemInSlot.item;
			}

			return null;
		}
		void SpawnNewItem(Item item, InventorySlot slot) 
		{
			GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
			InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
			inventoryItem.InitialiseItem(item);
		}

	}
}
