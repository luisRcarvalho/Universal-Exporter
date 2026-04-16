using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    private void OnEnable()
    {
        GameManager.OnScoreUpdated += UpdateScoreDisplay;
        GameManager.OnGameEnded += ShowGameOver;
    }
    private void OnDisable()
    {
        GameManager.OnScoreUpdated -= UpdateScoreDisplay;
        GameManager.OnGameEnded -= ShowGameOver;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        
        restartButton.onClick.AddListener(RestartGame);
    }

    public void UpdateScoreDisplay(float currentScore)
    {
        scoreText.text = $"Score: {Mathf.FloorToInt(currentScore)}";
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}