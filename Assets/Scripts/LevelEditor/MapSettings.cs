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

    [Header("Prefabs - Toggleable")]
    [Tooltip("切り替え壁（ボタンでオン/オフ）")]
    public GameObject toggleableWallPrefab;
    
    [Tooltip("壁ボタン（切り替え壁を制御）")]
    public GameObject wallButtonPrefab;

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
        // 数字 0-9 はボタン（チャンネル0-9）
        if (symbol >= '0' && symbol <= '9')
        {
            return wallButtonPrefab;
        }
        
        // 小文字 a-j は切り替え壁（チャンネル0-9、初期ON）
        if (symbol >= 'a' && symbol <= 'j')
        {
            return toggleableWallPrefab;
        }

        // 小文字 k-t は切り替え壁（チャンネル0-9、初期OFF）
        if (symbol >= 'k' && symbol <= 't')
        {
            return toggleableWallPrefab;
        }

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
    /// 記号からチャンネルIDを取得（ボタン/切り替え壁用）
    /// </summary>
    public static int GetChannelId(char symbol)
    {
        // 数字 0-9 → チャンネル 0-9
        if (symbol >= '0' && symbol <= '9')
        {
            return symbol - '0';
        }
        
        // 小文字 a-j → チャンネル 0-9（初期ON）
        if (symbol >= 'a' && symbol <= 'j')
        {
            return symbol - 'a';
        }

        // 小文字 k-t → チャンネル 0-9（初期OFF）
        if (symbol >= 'k' && symbol <= 't')
        {
            return symbol - 'k';
        }
        
        return -1; // チャンネル対象外
    }

    /// <summary>
    /// 記号がボタンかどうか
    /// </summary>
    public static bool IsWallButton(char symbol)
    {
        return symbol >= '0' && symbol <= '9';
    }

    /// <summary>
    /// 記号が切り替え壁かどうか
    /// </summary>
    public static bool IsToggleableWall(char symbol)
    {
        return (symbol >= 'a' && symbol <= 'j') || (symbol >= 'k' && symbol <= 't');
    }

    /// <summary>
    /// 切替壁が初期OFFかどうか
    /// </summary>
    public static bool IsToggleableWallInitiallyOff(char symbol)
    {
        return symbol >= 'k' && symbol <= 't';
    }

    /// <summary>
    /// 記号の説明を取得
    /// </summary>
    public static string GetSymbolDescription(char symbol)
    {
        // 数字 0-9 はボタン
        if (symbol >= '0' && symbol <= '9')
        {
            return $"ボタン{symbol}";
        }
        
        // 小文字 a-j は切り替え壁（初期ON）
        if (symbol >= 'a' && symbol <= 'j')
        {
            return $"切替壁{(char)('0' + (symbol - 'a'))}";
        }

        // 小文字 k-t は切り替え壁（初期OFF）
        if (symbol >= 'k' && symbol <= 't')
        {
            return $"切替壁{(char)('0' + (symbol - 'k'))}OFF";
        }

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
        return new char[] { 
            '#', ' ', 'S', 'G', '^', 'v', '<', '>', 'X', '_', 'B', 'D',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',  // ボタン
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',  // 切り替え壁（初期ON）
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't'   // 切り替え壁（初期OFF）
        };
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
