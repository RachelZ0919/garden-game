using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    LevelManager instance;
    static private int currentLevel = 1;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    static public void NextLevel()
    {
        currentLevel++;
        SceneManager.LoadScene(currentLevel);
    }
}
