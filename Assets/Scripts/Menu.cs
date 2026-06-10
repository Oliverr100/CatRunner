using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button playButton;
    public Button exitButton;


    private int sceneIndex = 1;

    private void Start()
    {
        playButton.onClick.AddListener(ChangeScene);
        exitButton.onClick.AddListener(ExitGame);
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
