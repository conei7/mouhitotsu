using UnityEngine;

/// <summary>
/// グリッドシステム - 座標変換とグリッド管理
/// </summary>
public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;

    public float GridSize => gridSize;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ワールド座標をグリッド座標に変換
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / gridSize);
        int y = Mathf.FloorToInt(worldPosition.y / gridSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// グリッド座標をワールド座標に変換（グリッドの中心）
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = gridPosition.x * gridSize + gridSize / 2f;
        float y = gridPosition.y * gridSize + gridSize / 2f;
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// マウス位置のグリッド座標を取得
    /// </summary>
    public Vector2Int GetMouseGridPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return WorldToGrid(mouseWorldPos);
    }

    /// <summary>
    /// マウス位置のワールド座標（グリッドにスナップ）を取得
    /// </summary>
    public Vector3 GetMouseWorldPositionSnapped()
    {
        return GridToWorld(GetMouseGridPosition());
    }
}
