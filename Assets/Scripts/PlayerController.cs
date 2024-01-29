using DG.Tweening;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Charging,
    Jumping
}

public class PlayerController : MonoBehaviour
{
    // 公共变量和序列化的私有变量
    public Transform Head;
    public Transform Body;
    public Rigidbody Rig;
    public GameObject Particle;

    [SerializeField] private float Force = 2;
    [SerializeField] private Vector3 maxScaleChange = new Vector3(1.5f, -1f, 1.5f);
    [SerializeField] private float maxPositionChange = -0.1f;
    [SerializeField] private float chargeDuration = 1f;

    public AudioManager audioManager;
    public StageManager stageManager;

    private Sequence chargeSequence;
    private Vector3 originalHeadPosition;
    private Vector3 originalBodyScale;
    private float StartAddForceTime;
    private Vector3 CameraRelativePosition;

    private bool isOnStage = true;
    private bool EnableInput = true;
    private float jumpInputCooldown = 1.0f; // 起跳后的输入冷却时间
    private float lastJumpTime = -1.0f; // 上次跳跃的时间

    private PlayerState currentState = PlayerState.Idle;

    public delegate void ChargeAction();
    public static event ChargeAction OnCharge;
    public static event ChargeAction OnChargeRelease;

    void Start()
    {
        originalHeadPosition = Head.localPosition;
        originalBodyScale = Body.localScale;
        CameraRelativePosition = Camera.main.transform.position - transform.position;
        audioManager = AudioManager.Instance;
        Rig.centerOfMass = Vector3.zero;
        Debug.Log(EnableInput);
    }

    void Update()
    {
        HandleState();
        // 更新跳跃计时器
        if (lastJumpTime >= 0 && (Time.time - lastJumpTime) > jumpInputCooldown)
        {
            lastJumpTime = -1.0f; // 重置计时器
        }
    }

    // Handle player state logic
    private void HandleState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdleState();
                break;
            case PlayerState.Charging:
                HandleChargingState();
                break;
            case PlayerState.Jumping:
                HandleJumpingState();
                break;
        }
    }


    private void HandleIdleState()
    {
        if (Rig.IsSleeping())
            Rig.WakeUp();
        if (!GameManager.Instance.IsGameOver && !IsFallingOrShaking())
            HandleInput();
    }

    private void HandleChargingState()
    {
        CheckForChargeRelease();
    }

    // 检查跳跃是否完成，如果完成则切换回 Idle 状态
    private void HandleJumpingState()
    {
        if (!IsFallingOrShaking() && isOnStage)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private bool IsFallingOrShaking()
    {
        return IsFalling() || IsShaking();
    }

    private bool IsFalling()
    {
        float fallingThreshold = -0.01f;
        return !isOnStage && Rig.velocity.y < fallingThreshold;
    }

    private bool IsShaking()
    {
        float shakingThreshold = 0.05f;
        return Rig.angularVelocity.magnitude > shakingThreshold;
    }

    private void HandleInput()
    {
        if (lastJumpTime >= 0) return;
        if (EnableInput && currentState == PlayerState.Idle)
        {
            if (Input.GetMouseButtonDown(0))
                StartCharge();
        }
    }

    private void StartCharge()
    {
        ChangeState(PlayerState.Charging);
        PerformChargeStartActions();
    }

    private void ChangeState(PlayerState newState)
    {
        currentState = newState;
    }

    private void PerformChargeStartActions()
    {
        StartAddForceTime = Time.time;
        Particle.SetActive(true);
        audioManager.Play("Energy");
        OnCharge?.Invoke();
        PrepareChargeAnimation();
    }

    private void EndCharge()
    {
        if (currentState == PlayerState.Charging)
        {
            ChangeState(PlayerState.Jumping);
            PerformChargeEndActions();
        }
    }

    private void CheckForChargeRelease()
    {
        if (!Input.GetMouseButton(0))
        {
            EndCharge();
        }
    }

    private void PerformChargeEndActions()
    {
        SetInputState(false);
        ResetChargeAnimation();
        ApplyChargeCompleteTransformations();
        CalculateAndExecuteJump();
    }

    private void SetInputState(bool state)
    {
        EnableInput = state;
    }

    private void PrepareChargeAnimation()
    {
        ResetChargeAnimation();
        Vector3 targetScale = originalBodyScale + maxScaleChange * 0.05f;
        float targetPositionY = originalHeadPosition.y + maxPositionChange;
        chargeSequence = DOTween.Sequence();
        chargeSequence.Append(Body.DOScale(targetScale, chargeDuration).SetEase(Ease.InOutSine));
        chargeSequence.Join(Head.DOLocalMoveY(targetPositionY, chargeDuration).SetEase(Ease.InOutSine));
    }

    private void ResetChargeAnimation()
    {
        if (chargeSequence != null && chargeSequence.IsActive())
            chargeSequence.Kill();
    }

    private void ApplyChargeCompleteTransformations()
    {
        Body.transform.DOScale(originalBodyScale, 0.2f);
        Head.transform.DOLocalMoveY(originalHeadPosition.y, 0.2f);
    }

    private void CalculateAndExecuteJump()
    {
        var elapse = Mathf.Min(Time.time - StartAddForceTime, 1);
        Particle.SetActive(false);
        OnJump(elapse);
        OnChargeRelease?.Invoke();
        audioManager.Stop("Energy");
    }

    private void OnJump(float jumpDuration)
    {
        Vector3 nextStagePosition = stageManager.GetNextStagePosition();
        Vector3 jumpDirection = (nextStagePosition - transform.position).normalized;
        jumpDirection.y = 0;
        Vector3 jumpForce = new Vector3(0, 5f, 0) + jumpDirection * jumpDuration * Force;
        Rig.AddForce(jumpForce, ForceMode.Impulse);
        PerformJumpRotation(jumpDirection);
        lastJumpTime = Time.time;
    }

    private void PerformJumpRotation(Vector3 jumpDirection)
    {
        if (Mathf.Abs(jumpDirection.x - 1f) < 0.3f)
        {
            transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
        }
        else
        {
            transform.DOLocalRotate(new Vector3(360, 0, 0), 0.6f, RotateMode.LocalAxisAdd);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Stage"))
            HandleLanding(collision.gameObject);
        else if (collision.transform.CompareTag("DeadZone"))
            PlayerDie();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Stage"))
        {
            isOnStage = false;
        }
    }

    private void HandleLanding(GameObject stage)
    {
        Rig.Sleep();
        SetInputState(true);
        isOnStage = true;
        Debug.Log("HandleLanding"+isOnStage);

        // 现在只有在着陆时才将状态改变为 Idle
        if (currentState == PlayerState.Jumping)
        {
            ChangeState(PlayerState.Idle);
        }

        stageManager.PlayerLandedOnStage(stage);
        MoveCamera();
    }

    private void PlayerDie()
    {
        DisablePlayerControl();
        audioManager.Play("Fall");
        GameManager.Instance.EndGame();
    }

    private void DisablePlayerControl()
    {
        SetInputState(false);
        Particle.SetActive(false);
        audioManager.Stop("Energy");
    }

    private void MoveCamera()
    {
        if (Camera.main != null && transform != null)
        {
            Camera.main.transform.DOMove(transform.position + CameraRelativePosition, 1);
        }
    }

    void OnDestroy()
    {
        DOTween.KillAll();
    }
}
