using UnityEngine;

/// <summary>
/// グリッド表示 - エディタのグリッド線を描画
/// </summary>
public class GridOverlay : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private Color originColor = new Color(1, 0, 0, 0.5f);
    [SerializeField] private int gridExtent = 50;

    private Material lineMaterial;

    private void Start()
    {
        CreateLineMaterial();
    }

    private void CreateLineMaterial()
    {
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void OnRenderObject()
    {
        if (Camera.current == null) return;
        if (lineMaterial == null) CreateLineMaterial();

        float size = GridSystem.Instance?.GridSize ?? 1f;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.LoadIdentity();
        GL.MultMatrix(Camera.current.worldToCameraMatrix);
        GL.LoadProjectionMatrix(Camera.current.projectionMatrix);

        GL.Begin(GL.LINES);

        // グリッド線（タイルの境界に合わせる）
        GL.Color(gridColor);
        for (int x = -gridExtent; x <= gridExtent; x++)
        {
            float xPos = x * size;
            GL.Vertex3(xPos, -gridExtent * size, 0);
            GL.Vertex3(xPos, gridExtent * size, 0);
        }
        for (int y = -gridExtent; y <= gridExtent; y++)
        {
            float yPos = y * size;
            GL.Vertex3(-gridExtent * size, yPos, 0);
            GL.Vertex3(gridExtent * size, yPos, 0);
        }

        // 原点（赤い線）
        GL.Color(originColor);
        GL.Vertex3(-gridExtent * size, 0, 0);
        GL.Vertex3(gridExtent * size, 0, 0);
        GL.Vertex3(0, -gridExtent * size, 0);
        GL.Vertex3(0, gridExtent * size, 0);

        GL.End();
        GL.PopMatrix();
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            DestroyImmediate(lineMaterial);
        }
    }
}

