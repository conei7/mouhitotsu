using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 重力インジケーターUI - 現在の重力方向と強さを視覚的に表示
/// 重ねがけ対応: 追加重力の強度も表示
/// </summary>
public class GravityIndicatorUI : MonoBehaviour
{
    [Header("Arrow Display")]
    [SerializeField] private RectTransform arrowPrimary;      // 通常重力の矢印
    [SerializeField] private RectTransform arrowSecondary;    // 追加重力の矢印
    [SerializeField] private RectTransform arrowCombined;     // 合成重力の矢印

    [Header("Arrow Colors")]
    [SerializeField] private Image arrowPrimaryImage;
    [SerializeField] private Image arrowSecondaryImage;
    [SerializeField] private Image arrowCombinedImage;
    [SerializeField] private Color primaryColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);    // グレー
    [SerializeField] private Color secondaryColor = new Color(0f, 1f, 1f, 0.8f);         // シアン
    [SerializeField] private Color combinedColor = new Color(1f, 0.5f, 0f, 1f);          // オレンジ
    [SerializeField] private bool autoColorSecondary = true;  // 追加重力の色を方向に応じて自動設定

    [Header("Settings")]
    [SerializeField] private float baseArrowLength = 30f;     // 基本の矢印の長さ（1倍時）
    [SerializeField] private float maxArrowLength = 150f;     // 最大の矢印の長さ（3倍以上対応）
    [SerializeField] private float referenceGravity = 9.81f;  // 基準となる重力値

    [Header("Text Display")]
    [SerializeField] private TextMeshProUGUI gravityInfoText; // 重力情報テキスト（オプション）

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 10f;         // 矢印の滑らかな回転速度

    private void Start()
    {
        // 色を設定
        if (arrowPrimaryImage != null) arrowPrimaryImage.color = primaryColor;
        if (arrowCombinedImage != null) arrowCombinedImage.color = combinedColor;
    }

    private void Update()
    {
        if (GravityController.Instance == null) return;

        UpdateArrows();
        UpdateInfoText();
    }

    private void UpdateArrows()
    {
        // 通常重力
        Vector2 primaryGrav = GravityController.Instance.PrimaryGravity;
        
        // 追加重力（重ねがけ後のベクトル）
        Vector2 secondaryGrav = GravityController.Instance.SecondaryGravityVector;
        bool hasSecondary = GravityController.Instance.HasSecondaryGravity;
        
        // 合成重力
        Vector2 combinedGrav = GravityController.Instance.CombinedGravity;

        // 通常重力の矢印を更新
        if (arrowPrimary != null)
        {
            UpdateArrow(arrowPrimary, primaryGrav, primaryGrav.magnitude);
        }

        // 追加重力の矢印を更新
        if (arrowSecondary != null)
        {
            if (hasSecondary)
            {
                arrowSecondary.gameObject.SetActive(true);
                
                // 重ねがけの強度を反映（magnitudeが強度を表す）
                float secondaryMagnitude = secondaryGrav.magnitude * referenceGravity;
                UpdateArrow(arrowSecondary, secondaryGrav, secondaryMagnitude);

                // 自動カラーリング
                if (autoColorSecondary && arrowSecondaryImage != null)
                {
                    arrowSecondaryImage.color = GetDirectionColor(secondaryGrav);
                }
            }
            else
            {
                arrowSecondary.gameObject.SetActive(false);
            }
        }

        // 合成重力の矢印を更新
        if (arrowCombined != null)
        {
            UpdateArrow(arrowCombined, combinedGrav, combinedGrav.magnitude);
        }
    }

    private void UpdateArrow(RectTransform arrow, Vector2 direction, float magnitude)
    {
        if (direction == Vector2.zero)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        
        arrow.gameObject.SetActive(true);

        // 方向から角度を計算（上が0度）
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        
        // 滑らかに回転
        Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);
        arrow.rotation = Quaternion.Slerp(arrow.rotation, targetRotation, smoothSpeed * Time.deltaTime);

        // 矢印の長さを重力の強さに応じて調整
        float normalizedMagnitude = magnitude / referenceGravity;
        float arrowLength = Mathf.Clamp(baseArrowLength * normalizedMagnitude, baseArrowLength * 0.3f, maxArrowLength);
        arrow.sizeDelta = new Vector2(arrow.sizeDelta.x, arrowLength);
    }

    private void UpdateInfoText()
    {
        if (gravityInfoText == null) return;

        Vector2 combined = GravityController.Instance.CombinedGravity;
        Vector2 secondary = GravityController.Instance.SecondaryGravityVector;
        float magnitude = GravityController.Instance.SecondaryGravityMagnitude;

        string directionName = GetDirectionName(combined);
        
        // 追加重力を矢印の繰り返しで表示（例: ↑↑↑）
        string stackArrows = GetStackArrows(secondary, magnitude);

        gravityInfoText.text = $"重力: {directionName}\n" +
                               $"強さ: {combined.magnitude:F1}\n" +
                               $"追加: {stackArrows}";
    }

    /// <summary>
    /// 重ねがけ数に応じて矢印を繰り返し生成
    /// </summary>
    private string GetStackArrows(Vector2 direction, float magnitude)
    {
        if (magnitude < 0.1f) return "OFF";

        string arrow = GetDirectionArrow(direction);
        int count = Mathf.RoundToInt(magnitude);  // 整数に丸める
        count = Mathf.Clamp(count, 1, 10);  // 最大10個まで

        return new string('_', 0).PadRight(0) + string.Concat(System.Linq.Enumerable.Repeat(arrow, count));
    }

    /// <summary>
    /// 方向から矢印文字を取得（4方向のみ）
    /// </summary>
    private string GetDirectionArrow(Vector2 direction)
    {
        if (direction.magnitude < 0.1f) return "";

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        // 最も強い方向を表示
        if (absX > absY)
        {
            return direction.x > 0 ? "→" : "←";
        }
        else
        {
            return direction.y > 0 ? "↑" : "↓";
        }
    }

    private string GetDirectionName(Vector2 direction)
    {
        if (direction.magnitude < 0.1f) return "無重力";

        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        // 最も強い方向を表示
        if (absX > absY)
        {
            return direction.x > 0 ? "→" : "←";
        }
        else
        {
            return direction.y > 0 ? "↑" : "↓";
        }
    }

    /// <summary>
    /// 方向に基づいて色を取得（HSVカラーホイール）
    /// </summary>
    private Color GetDirectionColor(Vector2 direction)
    {
        if (direction == Vector2.zero) return Color.gray;

        float angle = Mathf.Atan2(direction.y, direction.x);
        float hue = (angle + Mathf.PI) / (2 * Mathf.PI);
        
        // 強度で彩度を調整
        float saturation = Mathf.Clamp(direction.magnitude * 0.4f + 0.4f, 0.4f, 1f);
        
        return Color.HSVToRGB(hue, saturation, 1f);
    }
}
