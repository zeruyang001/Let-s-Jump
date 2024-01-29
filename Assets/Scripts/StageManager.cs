using DG.Tweening;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject[] stagePrefabs;
    public Transform currentStage;
    public PlayerController playerController;
    public AudioManager audioManager;

    private Transform nextStage;
    private Vector3 stageSpawnPosition;
    private Vector3 initialStagePosition;
    private Vector3 initialStageScale;
    private int reward = 1;

    private float maxDistance = 2f;
    private Vector3[] directions = { new Vector3(1, 0, 0), new Vector3(0, 0, 1) };
    private float scaleChangeAmount = 0.15f;
    private float positionChangeAmount = 0.15f;

    void Start()
    {
        if (stagePrefabs == null || stagePrefabs.Length == 0)
        {
            Debug.LogError("Stage prefabs are not set in StageManager");
            return;
        }

        audioManager = AudioManager.Instance;
        if (currentStage != null)
        {
            stageSpawnPosition = currentStage.position;
            initialStagePosition = currentStage.localPosition;
            initialStageScale = currentStage.localScale;
        }
        SpawnStage();
    }

    public void SpawnStage()
    {
        GameObject stagePrefab = stagePrefabs[Random.Range(0, stagePrefabs.Length)];
        Vector3 spawnDirection = directions[Random.Range(0, directions.Length)];
        stageSpawnPosition += spawnDirection * Random.Range(1.2f, maxDistance);

        nextStage = Instantiate(stagePrefab, stageSpawnPosition, Quaternion.identity).transform;
        SetRandomStageProperties(nextStage);
    }

    private void SetRandomStageProperties(Transform stage)
    {
        float randomScale = Random.Range(0.5f, 1);
        stage.localScale = new Vector3(randomScale, 0.5f, randomScale);
        stage.GetComponent<Renderer>().material.color = GetRandomColor();
    }

    private Color GetRandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }


    void OnEnable()
    {
        PlayerController.OnCharge += HandlePlayerCharge;
        PlayerController.OnChargeRelease += HandlePlayerChargeRelease;
    }

    void OnDisable()
    {
        PlayerController.OnCharge -= HandlePlayerCharge;
        PlayerController.OnChargeRelease -= HandlePlayerChargeRelease;
    }

    private void HandlePlayerCharge()
    {
        if (currentStage == null) return;

        Vector3 targetScale = new Vector3(currentStage.localScale.x, Mathf.Max(currentStage.localScale.y - scaleChangeAmount, 0.4f), currentStage.localScale.z);
        Vector3 targetPosition = new Vector3(currentStage.localPosition.x, currentStage.localPosition.y - positionChangeAmount, currentStage.localPosition.z);
        currentStage.DOScale(targetScale, 1f);
        currentStage.DOLocalMove(targetPosition, 1f);
    }

    private void HandlePlayerChargeRelease()
    {
        if (currentStage == null) return;

        currentStage.DOKill();
        currentStage.DOLocalMoveY(initialStagePosition.y, 0.4f);
        currentStage.DOScaleY(initialStageScale.y, 0.4f);
    }

    public void PlayerLandedOnStage(GameObject newStage)
    {
        if (currentStage != newStage.transform)
        {
            currentStage = newStage.transform;
            SpawnStage();
            CalculateScore(newStage.transform.position);
            audioManager.Play("Success");
        }
    }

    private void CalculateScore(Vector3 stagePosition)
    {
        Vector3 hitPos = playerController.transform.position;
        hitPos.y = stagePosition.y;
        float targetDistance = Vector3.Distance(hitPos, stagePosition);
        reward = targetDistance < 0.1f ? 2 * reward : 1;
        GameManager.Instance.AddScore(reward);
    }

    void OnDestroy()
    {
        DOTween.KillAll();
    }
    public Vector3 GetNextStagePosition()
    {
        // 返回下一个舞台的位置
        return nextStage.position;
    }
}
