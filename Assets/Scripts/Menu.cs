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
        SceneFader.EnsureExists();   // POLISH: fade in from black when the game starts

        playButton.onClick.AddListener(ChangeScene);

#if UNITY_WEBGL && !UNITY_EDITOR
        // POLISH: a browser tab cannot be closed with Application.Quit(), so the Exit button is hidden on WebGL
        if (exitButton != null) exitButton.gameObject.SetActive(false);
#else
        exitButton.onClick.AddListener(ExitGame);
#endif
    }

    public void ChangeScene()
    {
        SceneFader.Load(sceneIndex);   // POLISH: fade out, then load
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
