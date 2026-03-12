using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    public TextMeshProUGUI scoreText;

    public void AddScore()
    {
        score += 1;

        if (scoreText != null)
        {
            scoreText.text = "SCORE : " + score.ToString();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}