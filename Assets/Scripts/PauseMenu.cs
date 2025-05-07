using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        AudioListener.pause = true; // Pause all audio
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        AudioListener.pause = false; // Resume audio
    }

    public void Restart()
    {
        AudioListener.pause = false; // Ensure audio resumes on restart
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Home()
    {
        AudioListener.pause = false; // Resume audio before loading new scene
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }
}
