using UnityEngine;
using UnityEditor;

/// <summary>
/// MapGenerator のカスタムエディタ
/// ボタンでマップ生成・削除・プレビュー
/// </summary>
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator generator = (MapGenerator)target;

        // デフォルトのInspector表示
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // マップ情報表示
        if (generator.mapData != null)
        {
            EditorGUILayout.HelpBox(generator.GetMapInfo(), MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // バリデーションチェック
        bool hasErrors = false;
        if (generator.mapData == null)
        {
            EditorGUILayout.HelpBox("MapData を設定してください", MessageType.Warning);
            hasErrors = true;
        }
        else if (!generator.mapData.Validate(out string error))
        {
            EditorGUILayout.HelpBox($"マップエラー: {error}", MessageType.Error);
            hasErrors = true;
        }

        if (generator.mapSettings == null)
        {
            EditorGUILayout.HelpBox("MapSettings を設定してください", MessageType.Warning);
            hasErrors = true;
        }
        else if (!generator.mapSettings.Validate(out var errors))
        {
            foreach (var err in errors)
            {
                EditorGUILayout.HelpBox($"設定エラー: {err}", MessageType.Error);
            }
            hasErrors = true;
        }

        EditorGUILayout.Space(5);

        // ボタン
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = !hasErrors;
        if (GUILayout.Button("🔨 マップ生成", GUILayout.Height(35)))
        {
            generator.GenerateMap();
            EditorUtility.SetDirty(generator);
        }
        GUI.enabled = true;

        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("🗑 マップ削除", GUILayout.Height(35)))
        {
            generator.ClearMap();
            EditorUtility.SetDirty(generator);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // クイック作成ボタン
        EditorGUILayout.BeginHorizontal();
        
        if (generator.mapData == null)
        {
            if (GUILayout.Button("📄 新規MapData作成"))
            {
                CreateNewMapData(generator);
            }
        }
        
        if (generator.mapSettings == null)
        {
            if (GUILayout.Button("⚙ 新規MapSettings作成"))
            {
                CreateNewMapSettings(generator);
            }
        }
        
        EditorGUILayout.EndHorizontal();

        // 記号リファレンス
        EditorGUILayout.Space(10);
        if (EditorGUILayout.Foldout(true, "📖 記号リファレンス"))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("# = 壁/床    S = スタート    G = ゴール");
            EditorGUILayout.LabelField("^ = 上スイッチ    v = 下スイッチ");
            EditorGUILayout.LabelField("< = 左スイッチ    > = 右スイッチ");
            EditorGUILayout.LabelField("X = トゲ    _ = 落下ゾーン");
            EditorGUILayout.LabelField("B = 箱    D = 扉");
            EditorGUILayout.EndVertical();
        }
    }

    private void CreateNewMapData(MapGenerator generator)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "MapData 保存先",
            "Stage1",
            "asset",
            "MapData を保存する場所を選択してください",
            "Assets/Data/Maps"
        );

        if (!string.IsNullOrEmpty(path))
        {
            MapData newData = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.SaveAssets();
            generator.mapData = newData;
            EditorUtility.SetDirty(generator);
            Selection.activeObject = newData;
        }
    }

    private void CreateNewMapSettings(MapGenerator generator)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "MapSettings 保存先",
            "MapSettings",
            "asset",
            "MapSettings を保存する場所を選択してください",
            "Assets/Data"
        );

        if (!string.IsNullOrEmpty(path))
        {
            MapSettings newSettings = ScriptableObject.CreateInstance<MapSettings>();
            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            generator.mapSettings = newSettings;
            EditorUtility.SetDirty(generator);
            Selection.activeObject = newSettings;
        }
    }
}
