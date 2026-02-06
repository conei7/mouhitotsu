using UnityEngine;

/// <summary>
/// ステージデータ構造（JSON保存用）
/// </summary>
[System.Serializable]
public class StageData
{
    /// <summary>
    /// ユニークID（例: "stage_001", "user_abc123"）
    /// </summary>
    public string id;

    /// <summary>
    /// 表示名（例: "Stage 1", "My Custom Map"）
    /// </summary>
    public string name;

    /// <summary>
    /// マップテキストデータ
    /// </summary>
    public string mapText;

    /// <summary>
    /// 組み込みステージかどうか
    /// </summary>
    public bool isBuiltIn;

    /// <summary>
    /// ステージ番号（組み込みステージ用、ソート用）
    /// </summary>
    public int stageNumber;

    /// <summary>
    /// カメラサイズ（0 = 自動計算）
    /// </summary>
    public float cameraSize;

    /// <summary>
    /// カメラオフセット
    /// </summary>
    public float cameraOffsetX;
    public float cameraOffsetY;

    /// <summary>
    /// 作成日時（Unix timestamp）
    /// </summary>
    public long createdAt;

    /// <summary>
    /// カメラオフセットをVector2として取得
    /// </summary>
    public Vector2 CameraOffset => new Vector2(cameraOffsetX, cameraOffsetY);

    /// <summary>
    /// マップの幅（文字数）
    /// </summary>
    public int Width
    {
        get
        {
            if (string.IsNullOrEmpty(mapText)) return 0;
            string[] lines = mapText.Split('\n');
            int maxWidth = 0;
            foreach (var line in lines)
            {
                string trimmed = line.TrimEnd('\r');
                if (trimmed.Length > maxWidth) maxWidth = trimmed.Length;
            }
            return maxWidth;
        }
    }

    /// <summary>
    /// マップの高さ（行数）
    /// </summary>
    public int Height
    {
        get
        {
            if (string.IsNullOrEmpty(mapText)) return 0;
            return mapText.Split('\n').Length;
        }
    }

    /// <summary>
    /// マップを2D配列として取得
    /// </summary>
    public char[,] GetMapArray()
    {
        if (string.IsNullOrEmpty(mapText)) return new char[0, 0];

        string[] lines = mapText.Split('\n');
        int height = lines.Length;
        int width = Width;

        char[,] map = new char[width, height];

        for (int y = 0; y < height; y++)
        {
            string line = lines[height - 1 - y].TrimEnd('\r');  // 上下反転（Unityは下がY=0）
            for (int x = 0; x < width; x++)
            {
                if (x < line.Length)
                {
                    map[x, y] = line[x];
                }
                else
                {
                    map[x, y] = ' ';  // 行が短い場合は空白
                }
            }
        }

        return map;
    }

    /// <summary>
    /// バリデーション
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        errorMessage = "";

        if (string.IsNullOrEmpty(mapText))
        {
            errorMessage = "マップテキストが空です";
            return false;
        }

        bool hasStart = mapText.Contains("S");
        bool hasGoal = mapText.Contains("G");

        if (!hasStart)
        {
            errorMessage = "スタート地点(S)がありません";
            return false;
        }

        if (!hasGoal)
        {
            errorMessage = "ゴール(G)がありません";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 新しいステージデータを作成
    /// </summary>
    public static StageData Create(string id, string name, string mapText, bool isBuiltIn = false, int stageNumber = 0)
    {
        return new StageData
        {
            id = id,
            name = name,
            mapText = mapText,
            isBuiltIn = isBuiltIn,
            stageNumber = stageNumber,
            cameraSize = 0,
            cameraOffsetX = 0,
            cameraOffsetY = 0,
            createdAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}

/// <summary>
/// ステージデータのリスト（JSON保存用）
/// </summary>
[System.Serializable]
public class StageDataList
{
    public StageData[] stages;
}
