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

	public UnityEvent on_click_event = new UnityEvent();
	public UnityEvent chestOpened = new UnityEvent();

	public enum DropDirection
	{
		Up,
		Right,
		Down,
		Left
	}

	public DropDirection dropDirection = DropDirection.Up;

	private List<GameObject> ChosenLoot = new List<GameObject>();

	private bool IsBeingOpened = false;


	private Vector3 GenerateSpread(Vector3 originalPos)
	{
		Vector3 newSpread = new Vector3();
		float newSpreadX;
		float newSpreadY;
		switch (dropDirection)
		{
			case DropDirection.Up:
				newSpreadX = originalPos.x - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
				newSpreadY = originalPos.y + (Random.value * ItemsGenerationSpread)/2;
				newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
				break;
			case DropDirection.Right:
				newSpreadX = originalPos.x + (Random.value * ItemsGenerationSpread)/2;
				newSpreadY = originalPos.y - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
				newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
				break;
			case DropDirection.Down:
				newSpreadX = originalPos.x - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
				newSpreadY = originalPos.y - (Random.value * ItemsGenerationSpread)/2;
				newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
				break;
			case DropDirection.Left:
				newSpreadX = originalPos.x - (Random.value * ItemsGenerationSpread)/2;
				newSpreadY = originalPos.y - ItemsGenerationSpread + Random.value * ItemsGenerationSpread;
				newSpread = new Vector3(newSpreadX, newSpreadY, originalPos.z);
				break;
				


		}
		return  newSpread;


	}



    void Start()
    {
		int additionalItemsCount = Random.Range(0, ItemsToGenerateMax-ItemsToGenerateMin);
		for (int i = 0; i < (ItemsToGenerateMin + additionalItemsCount); i++) 
				ChosenLoot.Add(LootToChooseFrom[Random.Range(0, LootToChooseFrom.Length)]);
    }

	public void OpenChest()
	{
		int additionalStackableItemRange = Mathf.Abs(StackableItemsToGenerateMax - StackableItemsToGenerateMin);
		foreach(GameObject loot in ChosenLoot)
		{
			bool isLootStackable = loot.GetComponent<WorldItem>().item.stackable;
			int amountToSpawn = 1;
			if (isLootStackable) amountToSpawn = StackableItemsToGenerateMin + Random.Range(0, additionalStackableItemRange);
			for (int i = 0; i < amountToSpawn ; i++)
			{
				GameObject newLoot = Instantiate(loot, this.transform.position, Quaternion.identity);
				Vector3 newPosition = GenerateSpread(newLoot.transform.position);
				newLoot.transform.position = newPosition;
				spawnedObjects.Add(newLoot);
			}
		}

		Destroy(this.gameObject);
	}

	public void ReadKeyCount(int keyCount)
	{
		if (!IsBeingOpened) return;
		IsBeingOpened = false;
		if (keyCount <= 0) return;
		chestOpened.Invoke();
		OpenChest();

	}


	void OnMouseDown()
	{
		on_click_event.Invoke();
		IsBeingOpened = true;
	}

}
