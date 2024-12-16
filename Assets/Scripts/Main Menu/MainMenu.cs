using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string newGameScene;

    public GameObject continueButton;

    public Player player;


    // Start is called before the first frame update
    public void Start()
    {
        if (PlayerPrefs.HasKey("ContinueLevel"))
        {
            continueButton.SetActive(true);
        }

    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        SceneManager.LoadScene(newGameScene);
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Game Quit");
    }

    public void Continue()
    {
        player.gameObject.SetActive(true);
        player.transform.position = new Vector3(PlayerPrefs.GetFloat("PositionX"), PlayerPrefs.GetFloat("PositionY"), PlayerPrefs.GetFloat("PositionZ"));

        if (PlayerPrefs.HasKey("CanDoubleJump"))
        {
            if (PlayerPrefs.GetInt("CanDoubleJump") == 1)
            {
                GameManager.instance.CanDoubleJump = true;
            }
        }

        if (PlayerPrefs.HasKey("CanDash"))
        {
            if (PlayerPrefs.GetInt("CanDash") == 1)
            {
                GameManager.instance.CanDash = true;
            }
        }

        if (PlayerPrefs.HasKey("CanWallJump"))
        {
            if (PlayerPrefs.GetInt("CanWallJump") == 1)
            {
                GameManager.instance.CanWallJump = true;
            }
        }

        SceneManager.LoadScene(PlayerPrefs.GetString("ContinueLevel"));
    }
}
