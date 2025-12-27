using UnityEngine;
using UnityEditor;

/// <summary>
/// MapData のカスタムエディタ
/// マップのプレビューとバリデーション表示
/// </summary>
[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        MapData mapData = (MapData)target;

        // デフォルトのInspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // マップ情報
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("マップ情報", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"サイズ: {mapData.Width} x {mapData.Height}");
        EditorGUILayout.EndVertical();

        // バリデーション
        EditorGUILayout.Space(5);
        if (mapData.Validate(out string error))
        {
            EditorGUILayout.HelpBox("✓ マップは有効です", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"✗ {error}", MessageType.Error);
        }

        // オブジェクト統計
        EditorGUILayout.Space(5);
        if (EditorGUILayout.Foldout(true, "📊 オブジェクト統計"))
        {
            ShowObjectStats(mapData);
        }

        // ビジュアルプレビュー
        EditorGUILayout.Space(10);
        if (EditorGUILayout.Foldout(true, "🗺 マッププレビュー"))
        {
            ShowMapPreview(mapData);
        }

        // 記号リファレンス
        EditorGUILayout.Space(10);
        if (EditorGUILayout.Foldout(true, "📖 記号リファレンス"))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("# = 壁/床    (空白) = 空間");
            EditorGUILayout.LabelField("S = スタート    G = ゴール");
            EditorGUILayout.LabelField("^ = 上スイッチ    v = 下スイッチ");
            EditorGUILayout.LabelField("< = 左スイッチ    > = 右スイッチ");
            EditorGUILayout.LabelField("X = トゲ    _ = 落下ゾーン");
            EditorGUILayout.LabelField("B = 箱    D = 扉");
            EditorGUILayout.EndVertical();
        }
    }

    private void ShowObjectStats(MapData mapData)
    {
        if (string.IsNullOrEmpty(mapData.mapText)) return;

        char[,] map = mapData.GetMapArray();
        int walls = 0, switches = 0, hazards = 0;

        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                char c = map[x, y];
                if (c == '#') walls++;
                if (c == '^' || c == 'v' || c == '<' || c == '>') switches++;
                if (c == 'X' || c == '_') hazards++;
            }
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"壁/床: {walls}");
        EditorGUILayout.LabelField($"スイッチ: {switches}");
        EditorGUILayout.LabelField($"危険物: {hazards}");
        EditorGUILayout.LabelField($"スタート: {mapData.FindAll('S').Count}");
        EditorGUILayout.LabelField($"ゴール: {mapData.FindAll('G').Count}");
        EditorGUILayout.EndVertical();
    }

    private void ShowMapPreview(MapData mapData)
    {
        if (string.IsNullOrEmpty(mapData.mapText)) return;

        // プレビュー用のスタイル
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.font = Font.CreateDynamicFontFromOSFont("Consolas", 12);
        style.fontSize = 11;
        style.richText = true;

        // 色付きテキストを生成
        string coloredMap = "";
        foreach (char c in mapData.mapText)
        {
            coloredMap += GetColoredChar(c);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(coloredMap, style);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private string GetColoredChar(char c)
    {
        return c switch
        {
            '#' => "<color=#888888>#</color>",
            'S' => "<color=#00ff00>S</color>",
            'G' => "<color=#ffff00>G</color>",
            '^' => "<color=#00ffff>^</color>",
            'v' => "<color=#00ffff>v</color>",
            '<' => "<color=#00ffff><</color>",
            '>' => "<color=#00ffff>></color>",
            'X' => "<color=#ff0000>X</color>",
            '_' => "<color=#ff8800>_</color>",
            'B' => "<color=#8B4513>B</color>",
            'D' => "<color=#CD853F>D</color>",
            '\n' => "\n",
            _ => c.ToString()
        };
    }
}
