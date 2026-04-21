using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// エディタ用カメラ - パン/ズーム操作
/// </summary>
public class EditorCamera : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float panSpeedMultiplier = 2f; // Shift押下時

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float pinchZoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Touch Camera Control")]
    [SerializeField] private float touchPanSpeed = 1f;

    [Header("Mouse Pan")]
    [SerializeField] private bool enableMiddleMousePan = true;

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isPanning = false;
    private bool isPinching = false;
    private float lastPinchDistance = 0f;
    private Vector2 lastTouchMidpoint;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleTouchPanAndZoom();
        HandleMousePan();
        HandleZoom();
    }

    private void HandleKeyboardPan()
    {
        float speed = panSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= panSpeedMultiplier;
        }

        // カメラサイズに応じてスピード調整
        speed *= cam.orthographicSize / 5f;

        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            movement.y += speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            movement.y -= speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            movement.x -= speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            movement.x += speed * Time.deltaTime;

        transform.position += movement;
    }

    private void HandleMousePan()
    {
        if (!enableMiddleMousePan) return;

        // 中クリックでパン開始
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        // 中クリック離すとパン終了
        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        // パン中
        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 worldDelta = cam.ScreenToWorldPoint(Vector3.zero) - 
                                 cam.ScreenToWorldPoint(delta);
            transform.position += worldDelta;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void HandleTouchPanAndZoom()
    {
        if (!Input.touchSupported || Input.touchCount < 2)
        {
            isPinching = false;
            return;
        }

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (IsTouchOverUI(touch0) || IsTouchOverUI(touch1))
        {
            isPinching = false;
            return;
        }

        Vector2 currentMidpoint = (touch0.position + touch1.position) * 0.5f;
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        if (!isPinching || touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            isPinching = true;
            lastTouchMidpoint = currentMidpoint;
            lastPinchDistance = currentDistance;
            return;
        }

        Vector2 midpointDelta = currentMidpoint - lastTouchMidpoint;
        if (midpointDelta.sqrMagnitude > 0.01f)
        {
            Vector3 prevWorld = cam.ScreenToWorldPoint(new Vector3(lastTouchMidpoint.x, lastTouchMidpoint.y, cam.nearClipPlane));
            Vector3 currentWorld = cam.ScreenToWorldPoint(new Vector3(currentMidpoint.x, currentMidpoint.y, cam.nearClipPlane));
            Vector3 worldDelta = prevWorld - currentWorld;
            worldDelta.z = 0f;
            transform.position += worldDelta * touchPanSpeed;
        }

        float pinchDelta = currentDistance - lastPinchDistance;
        if (Mathf.Abs(pinchDelta) > 0.01f)
        {
            float newSize = cam.orthographicSize - pinchDelta * pinchZoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        lastTouchMidpoint = currentMidpoint;
        lastPinchDistance = currentDistance;
    }

    private bool IsTouchOverUI(Touch touch)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    /// <summary>
    /// カメラを指定位置に移動
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    /// <summary>
    /// カメラをリセット
    /// </summary>
    public void ResetCamera()
    {
        transform.position = new Vector3(0, 0, transform.position.z);
        cam.orthographicSize = 5f;
    }
}
