using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image slotImage; // Image компонент слота
    public Sprite defaultSlotSprite; // Обычный спрайт слота
    public Sprite selectedSlotSprite; // Спрайт для выбранного слота
    
    public void Select() {
        slotImage.sprite = selectedSlotSprite; // Меняем спрайт на "выбранный"
    }
    
    public void Deselect() {
        slotImage.sprite = defaultSlotSprite; // Возвращаем обычный спрайт
    }

    public void OnDrop(PointerEventData eventData) {
        if (transform.childCount == 0) {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }
}
