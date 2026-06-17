using Nova;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public UIBlock2D RestartButton;
    public UIBlock2D ExitButton;

    public GameManager gameManager;
    void Awake()
    {
        RestartButton.AddGestureHandler<Gesture.OnClick>(OnRestartClick);
        RestartButton.AddGestureHandler<Gesture.OnHover>(OnRestartHover);
        RestartButton.AddGestureHandler<Gesture.OnUnhover>(OnRestartUnhover);

        ExitButton.AddGestureHandler<Gesture.OnClick>(OnExitClick);
        ExitButton.AddGestureHandler<Gesture.OnHover>(OnExitHover);
        ExitButton.AddGestureHandler<Gesture.OnUnhover>(OnExitUnhover);
    }

    void OnRestartClick(Gesture.OnClick evt)
    {
        gameManager.RestartGame();
    }

    void OnRestartHover(Gesture.OnHover evt)
    {
        
    }

    void OnRestartUnhover(Gesture.OnUnhover evt)
    {
        
    }

    void OnExitClick(Gesture.OnClick evt)
    {
        gameManager.ExitGame();
    }

    void OnExitHover(Gesture.OnHover evt)
    {
        
    }

    void OnExitUnhover(Gesture.OnUnhover evt)
    {
        
    }
}
