using UnityEngine;

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
        if (canPickUp && other.CompareTag("Player"))
        {
            InventoryManager inventory = other.GetComponent<InventoryManager>();
            if (inventory.AddItem(item))
            {
                Destroy(gameObject);
            }
        }
    }
}