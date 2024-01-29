using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public enum GameState
{
    StartGame,
    InProgress,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnGameStateChanged; // 新增，游戏状态改变事件
                                                      
    public event Action<int> OnScoreChanged;// 定义一个代表分数变化的事件

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;

    private int score;
    public int Score
    {
        get { return score; }
        private set
        {
            score = value;
            // 这里可以添加更多逻辑，例如触发成就
        }
    }

    public bool IsGameOver { get; private set; }
    private GameState gameState = GameState.StartGame; // 添加游戏状态变量

    private void Awake()
    {
        InitializeSingleton();
        InitializeReferences();
    }

    private void Start()
    {
        StartGameSetup();
    }

    public void StartGame()
    {
        if (gameState != GameState.InProgress)
        {
            UpdateGameState(GameState.InProgress);
            ResetScore();
            audioManager.Play("Start");
        }
    }

    public void EndGame()
    {
        UpdateGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        DOTween.KillAll();
        StartCoroutine(ReloadScene());
    }

    public void AddScore(int amount)
    {
        ChangeScore(Score + amount);
    }

    public void AdReward()
    {
        Score *= 2;
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeReferences()
    {
        audioManager = audioManager ?? AudioManager.Instance;
        uiManager = uiManager ?? UIManager.Instance;
    }

    private void StartGameSetup()
    {
        audioManager.Play("Start");
    }

    private void ChangeScore(int newScore)
    {
        Score = newScore;
        OnScoreChanged?.Invoke(Score); // 触发事件
    }

    private void ResetScore()
    {
        ChangeScore(0);
    }

    private IEnumerator ReloadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return new WaitUntil(() => asyncLoad.isDone);
        StartGame();
    }

    private void UpdateGameState(GameState newState)
    {
        if (gameState != newState) // 只有在状态改变时才更新状态并触发事件
        {
            gameState = newState;
            OnGameStateChanged?.Invoke(gameState); // 触发状态改变事件

            switch (newState)
            {
                case GameState.InProgress:
                    SetGameState(false);
                    break;
                case GameState.GameOver:
                    SetGameState(true);
                    break;
            }
        }
    }

    private void SetGameState(bool isGameOver)
    {
        IsGameOver = isGameOver;
    }
}

