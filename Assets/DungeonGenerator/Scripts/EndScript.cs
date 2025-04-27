using UnityEngine;
using UnityEngine.Events;
using DungeonGenerator;


public class EndScript : MonoBehaviour
{
	public UnityEvent event_1 = new UnityEvent();
	void Update()
	{
		
	}
     void OnMouseDown(){
        // this object was clicked - do something
	event_1.Invoke();
 }
}
