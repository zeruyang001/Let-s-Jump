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
        gameManager.OnGameStateChanged += HandleGameStateChange; // �����¼�                                                     
        gameManager.OnScoreChanged += UpdateScore;// ���� GameManager �ķ��������¼�
        InitializeAdManager();
    }

    private void InitializeUI()
    {
        RestartButton.onClick.AddListener(() =>
        {
            // ���� GameManager �� RestartGame ����
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
                // ������Ϸ������Ļ��
                HideGameOverScreen();
                // ���� UI���������÷�����ʾ��
                UpdateScore(0);
                break;
            case GameState.GameOver:
                // ��ʾ��Ϸ������Ļ��
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
        ScoreText.text = "Score��" + newScore;
    }

    public void ShowGameOverScreen()
    {
        AdMobTips.gameObject.SetActive(false);
        GameOverScreen.SetActive(true);
        finalScore = GameManager.Instance.Score;
        FinalScore.text = "Final Score��" + finalScore;
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
            gameManager.OnGameStateChanged -= HandleGameStateChange; // ȡ�������¼�
            gameManager.OnScoreChanged -= UpdateScore;
        }
    }
}
