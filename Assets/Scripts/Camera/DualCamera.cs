using UnityEngine;

/// <summary>
/// 2体のキャラクターを追従するカメラ
/// </summary>
public class DualCamera : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform blueCharacter;
    [SerializeField] private Transform redCharacter;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10);
    [SerializeField] private float minSize = 5f;
    [SerializeField] private float maxSize = 10f;
    [SerializeField] private float sizeMultiplier = 0.5f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (blueCharacter == null || redCharacter == null) return;

        // 両キャラの中間点
        Vector3 midPoint = (blueCharacter.position + redCharacter.position) / 2f;
        Vector3 targetPos = midPoint + offset;

        // スムーズ追従
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // 距離に応じてズーム調整
        float distance = Vector2.Distance(blueCharacter.position, redCharacter.position);
        float targetSize = Mathf.Clamp(minSize + distance * sizeMultiplier, minSize, maxSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, smoothSpeed * Time.deltaTime);
    }
}
