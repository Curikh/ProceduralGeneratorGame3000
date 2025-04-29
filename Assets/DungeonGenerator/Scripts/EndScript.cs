using UnityEngine;
using UnityEngine.Events;


public class EndScript : MonoBehaviour
{
	public UnityEvent event_on_interaction = new UnityEvent();
	void Update()
	{
		
	}
     void OnMouseDown(){
        // this object was clicked - do something
	event_on_interaction.Invoke();
 }
}
