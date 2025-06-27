using UnityEngine;
using JoostenProductions;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SimpleClickerMovement : OverridableMonoBehaviour
{
    [Header("移动设置")]
    public bool isMoving = false;
    
    [Header("速度设置")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    private bool isRunning;                   // 是否处于跑步状态

    [Header("动画表现")]
    [SerializeField] private Animator runningManAnimator;
    [SerializeField] private SpriteRenderer runningManRenderer;

    [Header("音效")]
    public AudioClip footStepSounds;
    public AudioClip upgradeSound;

    [Header("输入设置")]
    private Vector2 moveInput;
    private bool runInput;
    
    // 等距投影速度修正系数 (√2/2 ≈ 0.707)
    private const float DIAGONAL_SPEED_MODIFIER = 0.707f;

    private Keyboard keyboard;

    void Start()
    {
        isRunning = false;
        keyboard = Keyboard.current;
    }

    public override void UpdateMe()
    {
        // 获取键盘输入
        if (keyboard != null)
        {
            moveInput = Vector2.zero;
            
            // WASD输入检测
            if (keyboard.wKey.isPressed) moveInput.y += 1f;
            if (keyboard.sKey.isPressed) moveInput.y -= 1f;
            if (keyboard.aKey.isPressed) moveInput.x -= 1f;
            if (keyboard.dKey.isPressed) moveInput.x += 1f;
            
            // Shift键跑步检测
            runInput = keyboard.leftShiftKey.isPressed;
        }
        else
        {
            moveInput = Vector2.zero;
            runInput = false;
        }

        // 检查是否在移动
        bool wasMoving = isMoving;
        isMoving = moveInput.magnitude > 0.1f;

        // 更新跑步状态
        bool wasRunning = isRunning;
        isRunning = isMoving && runInput;

        // 更新动画参数
        runningManAnimator.SetBool("isWalking", isMoving);
        runningManAnimator.SetBool("isRunning", isRunning);

        // 播放跑步升级音效（从走路切换到跑步时）
        if (isRunning && !wasRunning && isMoving)
        {
            runningManRenderer.color = Color.green;
            AudioManager.Instance.PlaySound(upgradeSound, 0.3f);
            runningManRenderer.DOColor(Color.white, 0.2f).SetDelay(0.1f);
        }

        // 移动角色
        if (isMoving)
        {
            MoveCharacter();
            
            // 如果刚开始移动或者移动方向改变，播放步伐效果
            if (!wasMoving || Vector2.Distance(moveInput.normalized, runningManAnimator.GetFloat("DirectionX") * Vector2.right + runningManAnimator.GetFloat("DirectionY") * Vector2.up) > 0.1f)
            {
                OnStep();
            }
        }

        // 更新动画方向参数
        UpdateAnimationDirection();
    }

    private void MoveCharacter()
    {
        Vector2 normalizedInput = moveInput.normalized;
        
        // 计算移动速度
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // 等距投影速度修正：斜向移动时减少速度以保持视觉一致性
        bool isDiagonal = Mathf.Abs(normalizedInput.x) > 0.1f && Mathf.Abs(normalizedInput.y) > 0.1f;
        if (isDiagonal)
        {
            currentSpeed *= DIAGONAL_SPEED_MODIFIER;
        }

        // 移动角色
        Vector3 movement = new Vector3(normalizedInput.x, normalizedInput.y, 0) * currentSpeed * Time.deltaTime;
        transform.position += movement;
    }

    private void UpdateAnimationDirection()
    {
        if (isMoving)
        {
            Vector2 normalizedInput = moveInput.normalized;
            runningManAnimator.SetFloat("DirectionX", normalizedInput.x);
            runningManAnimator.SetFloat("DirectionY", normalizedInput.y);
        }
    }

    private void OnStep()
    {
        // 角色进行步伐动画, 并设置初始大小
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.2f).OnComplete(() => transform.DOScale(Vector3.one, 0.1f));
        AudioManager.Instance.PlaySound(footStepSounds, 1, true);
    }
}
