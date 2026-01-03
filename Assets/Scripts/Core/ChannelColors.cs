using UnityEngine;

/// <summary>
/// チャンネル別の色を管理するユーティリティクラス
/// ボタンと切り替え壁で同じ色を使用して対応関係を視覚化
/// </summary>
public static class ChannelColors
{
    // 10チャンネル分の色パレット（赤・茶を回避）
    private static readonly Color[] colors = new Color[]
    {
        new Color(0f, 0.81f, 0.82f),      // 0: シアン (#00CED1)
        new Color(0.88f, 0.25f, 0.98f),   // 1: マゼンタ (#E040FB)
        new Color(0.46f, 1f, 0.01f),      // 2: ライム (#76FF03)
        new Color(1f, 0.84f, 0f),         // 3: ゴールド (#FFD600)
        new Color(0.33f, 0.43f, 0.99f),   // 4: インディゴ (#536DFE)
        new Color(1f, 0.44f, 0.26f),      // 5: コーラル (#FF7043)
        new Color(1f, 0.25f, 0.51f),      // 6: ピンク (#FF4081)
        new Color(0.25f, 0.77f, 1f),      // 7: スカイブルー (#40C4FF)
        new Color(0.39f, 1f, 0.86f),      // 8: ミント (#64FFDA)
        new Color(0.7f, 0.53f, 1f),       // 9: ラベンダー (#B388FF)
    };

    /// <summary>
    /// チャンネルIDから色を取得
    /// </summary>
    public static Color GetColor(int channelId)
    {
        if (channelId < 0 || channelId >= colors.Length)
        {
            return Color.white;
        }
        return colors[channelId];
    }

    /// <summary>
    /// 無効状態の色を取得（半透明）
    /// </summary>
    public static Color GetDisabledColor(int channelId)
    {
        Color c = GetColor(channelId);
        c.a = 0.4f;
        return c;
    }

    /// <summary>
    /// 押された状態の色を取得（暗め）
    /// </summary>
    public static Color GetPressedColor(int channelId)
    {
        Color c = GetColor(channelId);
        return new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, c.a);
    }

    /// <summary>
    /// チャンネル色の数を取得
    /// </summary>
    public static int ColorCount => colors.Length;

    /// <summary>
    /// チャンネル名を取得
    /// </summary>
    public static string GetChannelName(int channelId)
    {
        string[] names = { "シアン", "マゼンタ", "ライム", "ゴールド", "インディゴ", 
                          "コーラル", "ピンク", "スカイ", "ミント", "ラベンダー" };
        if (channelId < 0 || channelId >= names.Length)
        {
            return "不明";
        }
        return names[channelId];
    }
}
