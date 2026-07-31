using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GoblinController.PersistentMaxHealth = 2;
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
            Debug.Log("Выход из игры");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
}