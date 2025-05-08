using UnityEngine;
using JoostenProductions;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SimpleClickerMovement : OverridableMonoBehaviour
{
    [Header("点击设置")]
    private float clickingFactor = 0;
    public float factorDecreaseRate = 1f;
    public float factorIncreasePerClick = 0.75f;

    [Header("精准点击设置")]
    public float targetClickInterval = 0.5f;  // 目标点击间隔时间
    public float clickTolerance = 0.15f;       // 允许的误差范围
    public int requiredPerfectClicks = 5;     // 进入Dash所需的连续精准点击次数
    private float lastClickTime;              // 上次点击时间
    private int currentPerfectClicks;         // 当前连续精准点击次数
    private bool isRunning;                   // 是否处于Dash状态
    public Color perfectClickColor = Color.yellow;
    public ParticleSystem dashParticle;
    public ParticleSystem upgradeParticle;

    [Header("移动设置")]
    public bool isMoving = false;
    
    [Header("速度设置")]
    public float maxSpeed = 5f;
    public float minSpeed = 0.5f;
    private float currentSpeed = 0f;

    [Header("动画表现")]
    [SerializeField] private Animator runningManAnimator;
    [SerializeField] private SpriteRenderer runningManRenderer;

    [Header("音效")]
    public AudioClip footStepSounds;
    public AudioClip upgradeSound;

    private Mouse mouse;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = 0;
        lastClickTime = 0f;
        currentPerfectClicks = 0;
        isRunning = false;
        mouse = Mouse.current;
    }

    // Update is called once per frame
    public override void UpdateMe()
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            float currentTime = Time.time;
            float interval = currentTime - lastClickTime;
            
            // 检查是否是精准点击
            if (lastClickTime > 0 && Mathf.Abs(interval - targetClickInterval) <= clickTolerance)
            {
                currentPerfectClicks++;
                Color originalColor = runningManRenderer.color;
                if (currentPerfectClicks == requiredPerfectClicks)
                {
                    isRunning = true;
                    runningManAnimator.SetBool("isRunning", true);
                    // dashParticle.Play();
                    // upgradeParticle.Play();

                    runningManRenderer.color = Color.green;
                    AudioManager.Instance.PlaySound(upgradeSound, 0.3f);
                }
                else if (currentPerfectClicks < requiredPerfectClicks)
                {
                    runningManRenderer.color = perfectClickColor;
                }
                else
                {
                    runningManRenderer.color = Color.white;
                }
                runningManRenderer.DOColor(originalColor, 0.2f).SetDelay(0.1f);
            }
            else
            {
                // 如果不是精准点击，重置计数和状态
                currentPerfectClicks = 0;
                isRunning = false;
                runningManAnimator.SetBool("isRunning", false);
                // dashParticle.Stop();
            }

            lastClickTime = currentTime;
            clickingFactor += factorIncreasePerClick;
            // OnStep();
        }

        clickingFactor -= factorDecreaseRate * Time.deltaTime;
        clickingFactor = Mathf.Clamp01(clickingFactor);

        currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, clickingFactor);

        if (clickingFactor <= 0)
        {
            isMoving = false;
            runningManAnimator.SetBool("isWalking", false);

            // 如果不是精准点击，重置计数和状态
            currentPerfectClicks = 0;
            isRunning = false;
            runningManAnimator.SetBool("isRunning", false);
            // dashParticle.Stop();
        }
        else
        {
            isMoving = true;
            runningManAnimator.SetBool("isWalking", true);
        }

        if (isMoving)
        {
            float speedMultiplier = isRunning ? 2f : 1f;  // Dash状态下速度翻倍
            MoveTowardsMouse(currentSpeed * speedMultiplier);
        }

        RotateTowardsMouse();
    }

    private void MoveTowardsMouse(float speed)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;
        transform.position += (Vector3)directionToMouse * speed * Time.deltaTime;
    }

    private void RotateTowardsMouse()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized;
        runningManAnimator.SetFloat("DirectionX", directionToMouse.x);
        runningManAnimator.SetFloat("DirectionY", directionToMouse.y);
    }

     private void OnStep()
    {
        // 角色进行步伐动画, 并设置初始大小
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f).OnComplete(() => transform.DOScale(Vector3.one, 0.1f));
        AudioManager.Instance.PlaySound(footStepSounds, 1, true);
    }
}
