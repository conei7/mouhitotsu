using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// マップデータを保持するScriptableObject
/// Assets > Create > Mouhitotsu > Map Data で作成
/// </summary>
[CreateAssetMenu(fileName = "Stage1", menuName = "Mouhitotsu/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Map Definition")]
    [TextArea(15, 30)]
    [Tooltip("マップを文字で定義\n# = 壁, S = スタート, G = ゴール, ^v<> = 重力スイッチ, X = トゲ")]
    public string mapText = @"####################
#                  #
#              G   #
#              ### #
#                  #
#       ###        #
#       ###        #
#       ###        #
#                  #
# S   ^        >   #
####################";

    [Header("Stage Info")]
    public string stageName = "Stage 1";
    public int stageNumber = 1;

    [Header("Camera Settings")]
    [Tooltip("カメラサイズ（0 = 自動計算）")]
    public float cameraSize = 0;
    [Tooltip("カメラ位置オフセット")]
    public Vector2 cameraOffset = Vector2.zero;

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
                if (line.Length > maxWidth) maxWidth = line.Length;
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
            string line = lines[height - 1 - y];  // 上下反転（Unityは下がY=0）
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
    /// 特定の記号の位置を全て取得
    /// </summary>
    public List<Vector2Int> FindAll(char symbol)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        char[,] map = GetMapArray();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (map[x, y] == symbol)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        return positions;
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

        var starts = FindAll('S');
        var goals = FindAll('G');

        if (starts.Count == 0)
        {
            errorMessage = "スタート地点(S)がありません";
            return false;
        }

        if (starts.Count > 1)
        {
            errorMessage = "スタート地点(S)が複数あります";
            return false;
        }

        if (goals.Count == 0)
        {
            errorMessage = "ゴール(G)がありません";
            return false;
        }

        return true;
    }
}
