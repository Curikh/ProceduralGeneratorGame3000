using UnityEngine;

public class EndScript : MonoBehaviour
{
	void Update()
	{
		
	}
     void OnMouseDown(){
        // this object was clicked - do something
		Debug.Log("Clicked!");
		Destroy (this.gameObject);
 }
}
