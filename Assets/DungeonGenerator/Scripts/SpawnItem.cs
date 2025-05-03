       using UnityEngine;
       
       public class DemoScript : MonoBehaviour
       {
        public InventoryManager inventoryManager;
        public Item[] itemsToPickup;
        public void PickUpItem (int id) {
        inventoryManager.AddItem(itemsToPickup[id]);
        }
       }

