using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateSquareSprite : EditorWindow
{
    [MenuItem("Tools/Create 1x1 Square Sprite")]
    public static void Create()
    {
        int size = 128; // 128x128 pixels
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // 白で塗りつぶし
        Color[] colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);
        texture.Apply();

        // 保存
        string dirPath = Application.dataPath + "/Sprites";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        
        string path = dirPath + "/WhiteSquare.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());

        AssetDatabase.Refresh();

        // インポート設定
        string assetPath = "Assets/Sprites/WhiteSquare.png";
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 128; // 128px = 1 unit
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        Debug.Log("1x1 Square Sprite を作成しました: " + assetPath);
        
        // 選択状態にする
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
