using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Behavior cubeBehavior;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (!isGameOver && cubeBehavior.Hunger <= 0f)
        {
            TriggerGameOver();
        }
    }

    public bool CanSpawnFood()
    {
        return cubeBehavior.Hunger < 100f;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
