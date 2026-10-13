using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // POLISH: raised once when the cat dies (enemies listen to this to celebrate)
    public static event System.Action PlayerDied;

    [Header("Score Settings")]
    public int score = 0;
    public int highScore = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Difficulty Settings")]
    public float difficultyMultiplier = 1f;
    [Tooltip("Speed added to the enemies every 3 coins. The original game added 0.8, which made enemies faster than the cat after ~9 coins.")]
    public float difficultyStep = 0.25f;
    [Tooltip("Enemies never get faster than this multiplier. The original game had no cap.")]
    public float maxDifficulty = 2.2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip coinSound;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    [Tooltip("The big 'Game Over!' text of the Nova panel. It now also shows your score and best score.")]
    public Nova.TextBlock gameOverTitle;
    private bool isGameOver = false;

    [Header("Game Over Feel (polish)")]
    [Tooltip("Seconds (real time) the game freezes on the moment of impact.")]
    public float hitStopDuration = 0.09f;
    [Tooltip("Speed of the slow-motion death (1 = normal).")]
    public float slowMotionScale = 0.4f;
    [Tooltip("Seconds (real time) of slow motion before the Game Over panel appears.")]
    public float slowMotionDuration = 0.9f;

    [Header("Game over buttons")]
    public Button restartButton;
    public Button exitButton;

    private int startHighScore;
    private bool newBestAnnounced;

    public bool IsGameOver => isGameOver;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        startHighScore = highScore;
        gameOverPanel.SetActive(false);

        UpdateScoreUI();
        Time.timeScale = 1f;

        SceneFader.EnsureExists();   // POLISH: fade in from black
    }

    public void AddScore()
    {
        RegisterCoin(null);
    }

    // POLISH: the player passes the coin's position so we can spawn sparkles and a "+1" there
    public void AddScore(Vector3 coinPosition)
    {
        RegisterCoin(coinPosition);
    }

    private void RegisterCoin(Vector3? coinPosition)
    {
        if (isGameOver) return;

        score++;

        bool beatBest = false;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);

            // only celebrate if there was a real record to beat
            if (startHighScore > 0 && !newBestAnnounced)
            {
                newBestAnnounced = true;
                beatBest = true;
            }
        }

        UpdateScoreUI();
        UiPunch.Pop(scoreText, 0.35f, 0.4f);                       // POLISH: score text pops

        float pitch = 1f;
        var gf = GameFeel.Instance;
        if (gf != null && coinPosition.HasValue)
        {
            pitch = gf.CoinPickup(coinPosition.Value);              // POLISH: sparkle + "+1", returns combo pitch
        }

        if (coinSound != null)
        {
            SfxPlayer.Play(coinSound, 1f, pitch);                   // POLISH: coin sound climbs with quick combos
        }

        if (beatBest)
        {
            SfxPlayer.Play(ProceduralSfx.NewBest, 0.8f);
            UiPunch.Pop(highScoreText, 0.5f, 0.6f);
            if (highScoreText != null) highScoreText.color = new Color(1f, 0.82f, 0.25f);
            if (gf != null) gf.Banner("NEW BEST!", new Color(1f, 0.82f, 0.25f));
        }

        if (score % 3 == 0)
        {
            // POLISH: gentler, capped difficulty ramp
            difficultyMultiplier = Mathf.Min(maxDifficulty, difficultyMultiplier + difficultyStep);
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
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(GameOverRoutine());   // POLISH: dramatic death instead of an instant freeze
    }

    private IEnumerator GameOverRoutine()
    {
        PlayerDied?.Invoke();

        // 1) hit-stop: freeze everything for a split second on impact
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);

        // 2) slow motion so you can see the cat's death hop
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        // 3) freeze and show the Game Over panel
        Time.timeScale = 0f;
        PlayerPrefs.Save();
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        string best = highScore.ToString();
        bool newBest = score > startHighScore && score > 0;

        if (gameOverTitle != null)
        {
            // note: no <color> tags - Nova's text shader does not render them correctly
            string line2 = newBest
                ? "NEW BEST  " + score
                : "SCORE " + score + "     BEST " + best;
            gameOverTitle.Text = "Game Over!\n<size=40%>" + line2 + "</size>\n<size=60%>Try Again?</size>";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "FINAL SCORE: " + score.ToString();
        }

        if (newBest) SfxPlayer.Play(ProceduralSfx.NewBest, 0.8f);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;            // POLISH: the menu used to inherit timeScale 0 from the Game Over screen
        SceneFader.Load("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneFader.Load(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangeScene(int sceneIndex)
    {
        Time.timeScale = 1f;
        SceneFader.Load(sceneIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneFader.Load(0);
    }
    
}
