using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Score Settings")]
    private int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip coinSound;

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddScore()
    {
        score += 1;
        UpdateScoreDisplay();

        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound);
        }
    }

    private void UpdateScoreDisplay()
    {
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