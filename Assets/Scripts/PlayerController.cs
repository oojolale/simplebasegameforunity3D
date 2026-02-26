using UnityEngine;

/// <summary>
/// 玩家移动控制器 —— 键盘上下左右控制人物移动
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;        // 移动速度
    public float rotateSpeed = 10f;     // 转向速度（面朝移动方向）

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true; // 防止物理旋转让人物倒下
    }

    void FixedUpdate()
    {
        // 获取键盘输入（上下左右 / WASD）
        float horizontal = Input.GetAxisRaw("Horizontal"); // 左右：A/D 或 ←/→
        float vertical   = Input.GetAxisRaw("Vertical");   // 前后：W/S 或 ↑/↓

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // 移动
            Vector3 newPos = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // 平滑转向面朝移动方向
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}
