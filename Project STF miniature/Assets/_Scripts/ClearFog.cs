using UnityEngine;
using DG.Tweening;
using JoostenProductions;
using UnityEngine.InputSystem;
public class ClearFog : OverridableMonoBehaviour
{
    [Header("迷雾清除设置")]
    [SerializeField] private float clearRadius = 3f;      // 清除半径
    [SerializeField] private float radiusRatio = 0.6f;    // 椭圆Y轴压缩比例
    [SerializeField] private float fadeTime = 0.5f;       // 淡出时间
    [SerializeField] private LayerMask fogLayer;          // 迷雾层
    [SerializeField] private bool isPlayerMode = true;    // 是否为玩家模式

    private Camera mainCamera;
    private Mouse mouse;

    private void Start()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
    }

    public override void UpdateMe()
    {
        if (!isPlayerMode && mouse.rightButton.wasPressedThisFrame) // 右键点击
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition, fogLayer);
        
        if (hitCollider != null && hitCollider.CompareTag("Fog"))
        {
            ClearFogObject(hitCollider.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在Scene视图中绘制检测范围
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Matrix4x4 originalMatrix = Gizmos.matrix;
        
        // 创建椭圆变换矩阵
        Vector3 scale = new Vector3(1, radiusRatio, 1);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, scale);
        
        // 绘制扁椭圆
        Gizmos.DrawWireSphere(Vector3.zero, clearRadius);
        Gizmos.matrix = originalMatrix;
    }

    public override void FixedUpdateMe()
    {
        if (!isPlayerMode) return; // 如果不是玩家模式，不执行自动清除

        // 获取范围内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, clearRadius, fogLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Fog") && collider.gameObject.activeSelf)
            {
                // 获取碰撞体的世界坐标
                Vector2 fogPos = collider.transform.position;
                Vector2 currentPos = transform.position;

                // 计算在椭圆坐标系中的距离
                float xDist = fogPos.x - currentPos.x;
                float yDist = (fogPos.y - currentPos.y) / radiusRatio;
                float adjustedDistance = Mathf.Sqrt(xDist * xDist + yDist * yDist);

                // 如果在椭圆范围内
                if (adjustedDistance <= clearRadius)
                {
                    ClearFogObject(collider.gameObject);
                }
            }
        }
    }

    private void ClearFogObject(GameObject fogObject)
    {
        SpriteRenderer fogSprite = fogObject.GetComponent<SpriteRenderer>();
        if (fogSprite != null)
        {
            // 如果已经在淡出中，不重复处理
            if (fogSprite.color.a < 1f)
                return;

            // 淡出迷雾
            fogSprite.DOFade(0f, fadeTime).OnComplete(() =>
            {
                fogObject.SetActive(false);
            });
        }
    }
}
