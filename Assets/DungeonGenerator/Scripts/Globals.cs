using UnityEngine;

public class Globals : MonoBehaviour
{
	public int Seed;

	public static Globals Instance;

	public void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

}
