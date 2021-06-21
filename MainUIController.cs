using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    // Privates
    [SerializeField]
    private bool _paused, IsConfirmUp;

    [SerializeField]
    private CanvasGroup InGame, Paused, MenuConfirm, LevelComplete, Congrats, Story;

    [SerializeField]
    private float TextFadeTime, StoryFadeTime;

    // Protecteds

    // Publics

    // Start is called before the first frame update
    void Start()
    {
        _paused = false;
        Time.timeScale = 1;
        InitUI();
    }

    /// <summary>
    /// Initializes all our in game UI options.
    /// </summary>
    void InitUI()
    {
        // Verify InGame panel has been set.
        if (InGame == null)
            Debug.Log("InGame Panel has not been set!!!");
        else
        {
            InGame.alpha = 1;
            InGame.interactable = true;
        }

        // Verify Paused panel has been set.
        if (Paused == null)
            Debug.Log("Paused Panel has not been set!!!");
        else
        {
            Paused.alpha = 0;
            Paused.interactable = IsPaused();
        }

        // Verify MenuConfirmed panel has been set.
        if (MenuConfirm == null)
            Debug.Log("MenuConfirmed Panel has not been set!!!");
        else
        {
            MenuConfirm.alpha = 0;
            MenuConfirm.interactable = false;
            IsConfirmUp = false;
        }

        // Verify LevelComplete panel has been set.
        if (LevelComplete == null)
            Debug.Log("LevelComplete Panel has not been set!!!");
        else
        {
            LevelComplete.alpha = 0;
            LevelComplete.interactable = false;
        }

        // Verify Congrats panel has been set.
        if (Congrats == null)
            Debug.Log("Congrats Panel has not been set!!!");
        else
        {
            Congrats.alpha = 0;
        }

        // Verify Story panel has been set
        if (Story == null)
            Debug.Log("Story panel has not been set!!!");
        else
        {
            Story.alpha = 0;
        }
    }

    /// <summary>
    /// Called when we want to trigger a specific story text on screen.
    /// </summary>
    /// <param name="storyText">The text to display</param>
    public void TriggerStoryText(string storyText)
    {

        Text story = Story.GetComponentInChildren<Text>();
        if (story == null)
        {
            Debug.Log("Unable to get reference to story text!");
        }
        else
        {
            story.text = storyText;
            Story.alpha = 1;
            Debug.Log(string.Format("Running Story Trigger with text [{0}]", storyText));
            StartCoroutine(RunStoryRoutine(story));
        }
    }

    /// <summary>
    /// The <see cref="IEnumerator"/> to display and then hide the current desired story text
    /// </summary>
    /// <param name="story">The current story to display</param>
    /// <returns></returns>
    private IEnumerator RunStoryRoutine(Text story)
    {
        StartCoroutine(FadeTextToFullAlpha(StoryFadeTime, story));
        yield return new WaitForSeconds(StoryFadeTime);
        StartCoroutine(FadeTextToZeroAlpha(StoryFadeTime, story));
        yield return new WaitForSeconds(StoryFadeTime);
        Story.alpha = 0;
    }

    /// <summary>
    /// Fired when LevelComplete Next button is clicked
    /// </summary>
    public void NextLevel()
    {
        Debug.Log("Next Level button clicked!");
        GameManager.Instance.LoadNextLevel();
    }

    /// <summary>
    /// Fired when LevelComplete Replay button is clicked.
    /// </summary>
    public void ReplayLevel()
    {
        Debug.Log("Replay Level button clicked!");

        // Reset the paused button back to false if it is currently true.
        if (IsPaused())
            TogglePauseGame();

        GameManager.Instance.RestartCurrentLevel();
    }

    /// <summary>
    /// Triggered when the player triggers the end level trigger.
    /// </summary>
    public void LevelCompleted()
    {
        InGame.alpha = 0;
        InGame.interactable = false;

        Congrats.alpha = 1;
        StartCoroutine(FadeTextToFullAlpha(TextFadeTime, Congrats.GetComponentInChildren<Text>()));

        StartCoroutine(CompleteLevelEnumerator());
    }

    /// <summary>
    /// Runes the complete level enumerator to display the end of level buttons
    /// </summary>
    /// <returns></returns>
    private IEnumerator CompleteLevelEnumerator()
    {
        yield return new WaitForSeconds(TextFadeTime);

        LevelComplete.alpha = 1;
        LevelComplete.interactable = true;
        LevelComplete.blocksRaycasts = true;

        LevelComplete.transform.Find("B_Next").gameObject.GetComponent<Button>().interactable = GameManager.Instance.NextLevelIndexValid;
    }

    /// <summary>
    /// Used to fade a text into view.
    /// </summary>
    /// <param name="t">The time to take.</param>
    /// <param name="i">The text object to fade.</param>
    /// <returns></returns>
    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        Debug.Log(string.Format("Fading Text {0}", i.name));
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    /// <summary>
    /// Used to fade a text out of view.
    /// </summary>
    /// <param name="t">The time to take.</param>
    /// <param name="i">The text object to fade.</param>
    /// <returns></returns>
    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    /// <summary>
    /// Called when user clicks main menu button
    /// </summary>
    public void ReturnToMainMenu()
    {
        if (!IsConfirmUp)
        {
            MenuConfirm.alpha = 1;
            MenuConfirm.interactable = true;
            IsConfirmUp = true;

            InGame.interactable = false;
        }
        else
        {
            MenuConfirm.alpha = 0;
            MenuConfirm.interactable = false;
            IsConfirmUp = false;

            InGame.interactable = true;
        }
    }

    /// <summary>
    /// If user confirms yes, then return us to main menu
    /// </summary>
    public void ConfirmYes()
    {
        // Return us to the main menu
        Debug.Log("Confirmed Yes has been clicked.");
        GameManager.Instance.ReturnMainMenu();
    }

    /// <summary>
    /// If user confirms no, reset back to the pause menu.
    /// </summary>
    public void ConfirmNo()
    {
        // Go back to pause menu
        MenuConfirm.alpha = 0;
        MenuConfirm.interactable = false;
        InGame.interactable = true;
        IsConfirmUp = false;
        Debug.Log("Confirmed No has been clicked.");
    }

    /// <summary>
    /// Pauses/Unpauses the game
    /// </summary>
    public void TogglePauseGame()
    {
        // Toggle paused
        _paused = !_paused;

        if (IsPaused())
        {
            // Freeze the game.
            Time.timeScale = 0;
            Paused.alpha = 1;
            Paused.interactable = IsPaused();
            Debug.Log("Game Paused!");
        }
        else
        {
            // Resume the game
            Time.timeScale = 1;
            Paused.alpha = 0;
            Paused.interactable = IsPaused();
            Debug.Log("Game Unpaused!");
        }
    }

    /// <summary>
    /// Returns whether we are paused or not.
    /// </summary>
    /// <returns></returns>
    public bool IsPaused()
    {
        return _paused;
    }
}
