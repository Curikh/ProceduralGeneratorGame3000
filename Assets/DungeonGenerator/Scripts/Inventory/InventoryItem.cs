using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory{

	public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Image image;
		public Text countText;
		[HideInInspector] public Item item;
		[HideInInspector] public int count = 1;
		[HideInInspector] public Transform parentAfterDrag;

		public void InitialiseItem(Item newItem)
		{
			item = newItem;
			image.sprite = item.image;
			RefreshCount();
		}

		public void RefreshCount()
		{
			countText.text = count.ToString();
			countText.gameObject.SetActive(count > 1);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			image.raycastTarget = false;
			parentAfterDrag = transform.parent;
			transform.SetParent(transform.root); // UI item выносится на передний план
		}

		public void OnDrag(PointerEventData eventData)
		{
			transform.position = Input.mousePosition;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			image.raycastTarget = true;

			bool droppedOnSlot = eventData.pointerEnter != null &&
				eventData.pointerEnter.GetComponentInParent<InventorySlot>() != null;

			if (droppedOnSlot)
			{
				transform.SetParent(parentAfterDrag);
			}
			else
			{
				// Предмет выбрасывается — находим InventoryManager и вызываем DropItem
				InventorySlot originalSlot = parentAfterDrag.GetComponent<InventorySlot>();
				InventoryManager inventory = FindObjectOfType<InventoryManager>();
				inventory.DropItem(originalSlot);

				Destroy(gameObject); // удаляем UI
			}
		}
	}
}
