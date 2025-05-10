using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class InGameMenuCanvas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Resume() { Time.timeScale = 1;}

    public void ToMainMenu()
    {
        Resume();
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0) Time.timeScale = 1;
            else Time.timeScale = 0;
        }
        if (Time.timeScale == 0)  GetComponent<Canvas>().enabled = true;
        else    GetComponent<Canvas>().enabled = false;
        
    }
}
