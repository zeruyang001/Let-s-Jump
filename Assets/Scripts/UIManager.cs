using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text ScoreText;
    public GameObject GameOverScreen;
    public Text FinalScore;
    public Button RestartButton;

    private GameManager gameManager;
    private AdManager adManager;
    private Transform AdMobTips;
    private int finalScore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        InitializeUI();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.OnGameStateChanged += HandleGameStateChange; // 订阅事件                                                     
        gameManager.OnScoreChanged += UpdateScore;// 订阅 GameManager 的分数更新事件
        InitializeAdManager();
    }

    private void InitializeUI()
    {
        RestartButton.onClick.AddListener(() =>
        {
            // 调用 GameManager 的 RestartGame 方法
            GameManager.Instance.RestartGame();
        });
        RestartButton.gameObject.SetActive(false);
    }

    private void HandleGameStateChange(GameState newState)
    {
        switch (newState)
        {
            case GameState.StartGame:

                break;
            case GameState.InProgress:
                // 隐藏游戏结束屏幕等
                HideGameOverScreen();
                // 更新 UI，例如重置分数显示等
                UpdateScore(0);
                break;
            case GameState.GameOver:
                // 显示游戏结束屏幕等
                OpensAdMobTips();
                break;
        }
    }

    private void InitializeAdManager()
    {
        GameObject adManagerObj = GameObject.Find("AdManager") ?? Instantiate(new GameObject("AdManager"));
        adManager = adManagerObj.GetComponent<AdManager>() ?? adManagerObj.AddComponent<AdManager>();

        AdMobTips = GameObject.Find("Canvas").transform.Find("AdMobTips");
        if (AdMobTips != null)
        {
            Button LookBtn = AdMobTips.Find("LookBtn").GetComponent<Button>();
            Button CancelBtn = AdMobTips.Find("CancelBtn").GetComponent<Button>();

            LookBtn.onClick.AddListener(adManager.ShowRewardedAd);
            CancelBtn.onClick.AddListener(ShowGameOverScreen);
        }
        else
        {
            Debug.LogError("AdMobTips not found in the scene.");
        }
    }

    public void UpdateScore(int newScore)
    {
        ScoreText.text = "Score：" + newScore;
    }

    public void ShowGameOverScreen()
    {
        AdMobTips.gameObject.SetActive(false);
        GameOverScreen.SetActive(true);
        finalScore = GameManager.Instance.Score;
        FinalScore.text = "Final Score：" + finalScore;
        FinalScore.gameObject.SetActive(true);
        StartCoroutine(ActivateRestartButtonAfterDelay(1.0f));
    }

    public void HideGameOverScreen()
    {
        GameOverScreen.SetActive(false);
        FinalScore.gameObject.SetActive(false);
        RestartButton.gameObject.SetActive(false);
    }

    private IEnumerator ActivateRestartButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartButton.gameObject.SetActive(true);
    }

    public void OpensAdMobTips()
    {
        AdMobTips.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChange; // 取消订阅事件
            gameManager.OnScoreChanged -= UpdateScore;
        }
    }
}
