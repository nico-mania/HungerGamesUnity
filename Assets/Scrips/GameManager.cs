using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Behavior cubeBehavior;
    public GameObject gameOverUI;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (cubeBehavior.hunger <= 0f)
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f; // Pausiere das Spiel
    }

    public bool CanSpawnFood()
    {
        return cubeBehavior.hunger < 100f;
    }
}
