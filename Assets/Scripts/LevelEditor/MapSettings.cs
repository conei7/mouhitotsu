using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// マップ生成設定 - 記号とプレハブの対応を定義
/// </summary>
[CreateAssetMenu(fileName = "MapSettings", menuName = "Mouhitotsu/Map Settings")]
public class MapSettings : ScriptableObject
{
    [Header("Grid Settings")]
    [Tooltip("1グリッドのサイズ（ユニット）")]
    public float gridSize = 1f;

    [Header("Prefabs - Required")]
    [Tooltip("壁/床ブロック")]
    public GameObject wallPrefab;
    
    [Tooltip("プレイヤー")]
    public GameObject playerPrefab;
    
    [Tooltip("ゴール")]
    public GameObject goalPrefab;

    [Header("Prefabs - Gravity Switches")]
    [Tooltip("上方向重力スイッチ")]
    public GameObject switchUpPrefab;
    
    [Tooltip("下方向重力スイッチ")]
    public GameObject switchDownPrefab;
    
    [Tooltip("左方向重力スイッチ")]
    public GameObject switchLeftPrefab;
    
    [Tooltip("右方向重力スイッチ")]
    public GameObject switchRightPrefab;

    [Header("Prefabs - Hazards")]
    [Tooltip("トゲ")]
    public GameObject spikePrefab;
    
    [Tooltip("落下ゾーン")]
    public GameObject fallZonePrefab;

    [Header("Prefabs - Optional (Future)")]
    [Tooltip("押せる箱")]
    public GameObject boxPrefab;
    
    [Tooltip("扉")]
    public GameObject doorPrefab;

    [Header("Visual Settings")]
    [Tooltip("壁の色")]
    public Color wallColor = Color.white;
    
    [Tooltip("背景色")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);

    /// <summary>
    /// 記号からプレハブを取得
    /// </summary>
    public GameObject GetPrefab(char symbol)
    {
        return symbol switch
        {
            '#' => wallPrefab,
            'S' => playerPrefab,
            'G' => goalPrefab,
            '^' => switchUpPrefab,
            'v' => switchDownPrefab,
            '<' => switchLeftPrefab,
            '>' => switchRightPrefab,
            'X' => spikePrefab,
            '_' => fallZonePrefab,
            'B' => boxPrefab,
            'D' => doorPrefab,
            _ => null
        };
    }

    /// <summary>
    /// 記号の説明を取得
    /// </summary>
    public static string GetSymbolDescription(char symbol)
    {
        return symbol switch
        {
            '#' => "壁/床",
            ' ' => "空間",
            'S' => "スタート",
            'G' => "ゴール",
            '^' => "上スイッチ",
            'v' => "下スイッチ",
            '<' => "左スイッチ",
            '>' => "右スイッチ",
            'X' => "トゲ",
            '_' => "落下ゾーン",
            'B' => "箱",
            'D' => "扉",
            _ => "不明"
        };
    }

    /// <summary>
    /// 対応している全記号を取得
    /// </summary>
    public static char[] GetAllSymbols()
    {
        return new char[] { '#', ' ', 'S', 'G', '^', 'v', '<', '>', 'X', '_', 'B', 'D' };
    }

    /// <summary>
    /// 設定のバリデーション
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (wallPrefab == null) errors.Add("壁プレハブが未設定");
        if (playerPrefab == null) errors.Add("プレイヤープレハブが未設定");
        if (goalPrefab == null) errors.Add("ゴールプレハブが未設定");

        return errors.Count == 0;
    }
}
