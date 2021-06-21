using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = new GameManager();
            return instance;
        }
    }

    #endregion

    // Privates

    // Protected

    // Publics

    #region Properties

    /// <summary>
    /// Property to check whether or not the nextscene index is a valid scene in the build settings
    /// </summary>
    public bool NextLevelIndexValid
    {
        get
        {
            //Debug.Log(string.Format("Next scene index is: {0} /nBuildSettings scene count is: {1}", SceneManager.GetActiveScene().buildIndex + 1, SceneManager.sceneCountInBuildSettings));
            //Debug.Log(string.Format("The returned Bool Value is: {0}", SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings));
            return CurrentLevelIndex + 1 < SceneManager.sceneCountInBuildSettings;
        }
    }

    public int CurrentLevelIndex
    {
        get { return SceneManager.GetActiveScene().buildIndex; }
    } 

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Resources.LoadAll("LevelSettings");
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    /// <summary>
    /// Returns us back to the main menu of the game.
    /// </summary>
    public void ReturnMainMenu()
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(string.Format("Scenes/MainMenu"));
        SceneManager.LoadSceneAsync(sceneIndex);
    }

    /// <summary>
    /// Loads the next scene in the list if it is valid.
    /// </summary>
    public void LoadNextLevel()
    {
        // Check that index is within the scene count.
        if (NextLevelIndexValid)
        {
            Debug.Log(string.Format("Loading next scene :: {0} ::", SceneManager.GetSceneByBuildIndex(CurrentLevelIndex + 1).name));
            SceneManager.LoadSceneAsync(CurrentLevelIndex + 1);
        }
        else
        {
            Debug.LogError(string.Format("The scene index was {0}, which was not a valid index to load in {1}", CurrentLevelIndex + 1, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }
    }
    
    /// <summary>
    /// Restart current level.
    /// </summary>
    public void RestartCurrentLevel()
    {
        Debug.Log(string.Format("Restarting {0}", SceneManager.GetActiveScene().name));
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads a scene by the given name.
    /// </summary>
    /// <param name="name">The name of the scene.</param>
    public void LoadSceneByName(string name)
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(string.Format("Scenes/{0}", name));
            //SceneManager.GetSceneByName(string.Format("scene/{0}", name)).buildIndex;
        Debug.Log(string.Format("Loading scene [Index: {0}, Name: {1}]", sceneIndex, name));
        SceneManager.LoadSceneAsync(sceneIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
