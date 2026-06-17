using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int score = 0;
    public int highScore = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Difficulty Settings")]
    public float difficultyMultiplier = 1f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip coinSound;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    private bool isGameOver = false;

    [Header("Game over buttons")]
    public Button restartButton;
    public Button exitButton;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        gameOverPanel.SetActive(false);

        UpdateScoreUI();
        Time.timeScale = 1f;

    }

    public void AddScore()
    {
        if (isGameOver) return;

        score++;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        UpdateScoreUI();

        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound);
        }

        if (score % 3 == 0)
        {
            difficultyMultiplier += 0.8f;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE : " + score.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = "BEST : " + highScore.ToString();
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "FINAL SCORE: " + score.ToString();
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    
}