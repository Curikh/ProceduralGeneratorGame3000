using UnityEngine;

namespace Inventory
{
	public class WorldItem : MonoBehaviour
	{
		public Item item;
		public int amount = 1;
		private bool canPickUp = false;
		private float pickupCooldown = 0.5f;

		private void Start()
		{
			GetComponent<SpriteRenderer>().sprite = item.image;
			StartCoroutine(EnablePickup());
		}

		private System.Collections.IEnumerator EnablePickup()
		{
			yield return new WaitForSeconds(pickupCooldown);
			canPickUp = true;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.CompareTag("Player"))
			{
				if (item.type == ItemType.Coin)
				{
					PlayerCoins playerCoins = other.GetComponent<PlayerCoins>();
					if (playerCoins != null)
					{
						for (int i = 0; i < amount; i++)
						{
							playerCoins.AddCoin();
						}
					}

					Destroy(gameObject); // удаляем монету из мира
				}
					else if (item.type == ItemType.Key)
				{
					PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
					if (playerKeys != null)
					{
						for (int i = 0; i < amount; i++)
						{
							playerKeys.AddKey();
						}
					}

					Destroy(gameObject); // удаляем монету из мира
				}
				else
				{
					InventoryManager inventory = other.GetComponent<InventoryManager>();
					if (inventory != null)
					{
						bool wasPickedUp = inventory.AddItem(item);
						if (wasPickedUp)
						{
							Destroy(gameObject); // удаляем обычный предмет
						}
					}
				}
			}
		}
	}
}
