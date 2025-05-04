using UnityEngine;
using UnityEngine.Events;

public class ChestScript : MonoBehaviour
{

	[Header("Loot Variations")]
	[SerializeField] private GameObject[] LootToChooseFrom;
	public UnityEvent on_click_event = new UnityEvent();


	private GameObject ChosenLoot;




    void Start()
    {
		ChosenLoot = LootToChooseFrom[Random.Range(0, LootToChooseFrom.Length)];
        
    }

	void OnMouseDown()
	{
		Instantiate(ChosenLoot, this.transform.position, Quaternion.identity);
		Destroy(this.gameObject);
	}

}
