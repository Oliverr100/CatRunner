using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Difficulty Settings")]
    public float difficultyMultiplier = 1f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip coinSound;

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();

        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound);
        }

        if (score % 5 == 0)
        {
            difficultyMultiplier += 0.2f;
            Debug.Log("SPEED UP! New Multiplier: " + difficultyMultiplier);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE : " + score.ToString();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}