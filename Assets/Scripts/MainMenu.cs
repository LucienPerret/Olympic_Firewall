using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GoToScene(string sceneName)
    {
        SoundManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApp()
    {
        SoundManager.Instance?.PlayButtonClick();
        Application.Quit();
        Debug.Log("Application has quit");
    }
}
