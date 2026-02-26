using UnityEngine;

/// <summary>
/// 摄像机跟随玩家（固定偏移，俯视第三人称）
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;              // 跟随目标（Player）
    public Vector3 offset = new Vector3(0f, 8f, -6f);  // 摄像机相对偏移
    public float smoothSpeed = 8f;        // 跟随平滑速度

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.0f);
    }
}
