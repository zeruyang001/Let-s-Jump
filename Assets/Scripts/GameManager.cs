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

    public event Action<GameState> OnGameStateChanged; // ��������Ϸ״̬�ı��¼�
                                                      
    public event Action<int> OnScoreChanged;// ����һ����������仯���¼�

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;

    private int score;
    public int Score
    {
        get { return score; }
        private set
        {
            score = value;
            // ���������Ӹ����߼������紥���ɾ�
        }
    }

    public bool IsGameOver { get; private set; }
    private GameState gameState = GameState.StartGame; // �����Ϸ״̬����

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
        OnScoreChanged?.Invoke(Score); // �����¼�
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
        if (gameState != newState) // ֻ����״̬�ı�ʱ�Ÿ���״̬�������¼�
        {
            gameState = newState;
            OnGameStateChanged?.Invoke(gameState); // ����״̬�ı��¼�

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

