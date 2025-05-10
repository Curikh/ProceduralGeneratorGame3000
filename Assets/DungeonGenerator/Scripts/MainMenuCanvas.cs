using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuCanvas : MonoBehaviour
{
	public void StartGame()
	{
		string seedString = transform.Find("SeedInput").GetComponent<TMPro.TMP_InputField>().text;
		int seed;
		if (seedString == "") seed = 0;
		else
		{
			try
			{
				seed = int.Parse(seedString);
			}
			catch
			{
				return;
			}
		}

		Globals.Instance.Seed = seed;
		SceneManager.LoadScene("DemoScene", LoadSceneMode.Single);
	}

	public void ExitGame()
	{
		Debug.Log("Quitting...");
		Application.Quit();
	}
}
