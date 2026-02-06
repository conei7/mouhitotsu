using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ステージデータベース - 組み込み＆ユーザーステージ管理
/// </summary>
public class StageDatabase : MonoBehaviour
{
    public static StageDatabase Instance { get; private set; }

    private const string USER_STAGES_KEY = "UserStages";
    private const string BUILTIN_STAGES_RESOURCE = "BuiltInStages";

    private List<StageData> builtInStages = new List<StageData>();
    private List<StageData> userStages = new List<StageData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBuiltInStages();
        LoadUserStages();
    }

    /// <summary>
    /// 組み込みステージをResourcesから読み込み
    /// </summary>
    private void LoadBuiltInStages()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(BUILTIN_STAGES_RESOURCE);
        if (jsonAsset != null)
        {
            try
            {
                StageDataList list = JsonUtility.FromJson<StageDataList>(jsonAsset.text);
                if (list?.stages != null)
                {
                    builtInStages = new List<StageData>(list.stages);
                    foreach (var stage in builtInStages)
                    {
                        stage.isBuiltIn = true;
                    }
                    Debug.Log($"Loaded {builtInStages.Count} built-in stages");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load built-in stages: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Built-in stages file not found. Creating default stages.");
            CreateDefaultBuiltInStages();
        }
    }

    /// <summary>
    /// デフォルトの組み込みステージを作成
    /// </summary>
    private void CreateDefaultBuiltInStages()
    {
        builtInStages = new List<StageData>
        {
            StageData.Create("stage_001", "Stage 1", @"####################
#                  #
#                  #
#        #        G#
#                ###
#                  #
#                  #
#       ###        #
#S      ###        #
#     > ###    ^   #
####################", true, 1),

            StageData.Create("stage_002", "Stage 2", @"#########################
#                       #
#                    G  #
#                   ### #
#                       #
#                       #
#        ###            #
#        ###       ###  #
#                       #
#   <            >      #
#  ###          ###     #
#                       #
#      ^    ^    ^      #
# S   ###  ###  ###     #
#########################", true, 2)
        };
    }

    /// <summary>
    /// ユーザーステージをPlayerPrefsから読み込み
    /// </summary>
    private void LoadUserStages()
    {
        if (PlayerPrefs.HasKey(USER_STAGES_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(USER_STAGES_KEY);
                StageDataList list = JsonUtility.FromJson<StageDataList>(json);
                if (list?.stages != null)
                {
                    userStages = new List<StageData>(list.stages);
                    Debug.Log($"Loaded {userStages.Count} user stages");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load user stages: {e.Message}");
                userStages = new List<StageData>();
            }
        }
    }

    /// <summary>
    /// ユーザーステージをPlayerPrefsに保存
    /// </summary>
    private void SaveUserStages()
    {
        StageDataList list = new StageDataList { stages = userStages.ToArray() };
        string json = JsonUtility.ToJson(list, true);
        PlayerPrefs.SetString(USER_STAGES_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 全ステージを取得（組み込み + ユーザー）
    /// </summary>
    public List<StageData> GetAllStages()
    {
        var all = new List<StageData>();
        all.AddRange(builtInStages.OrderBy(s => s.stageNumber));
        all.AddRange(userStages.OrderByDescending(s => s.createdAt));
        return all;
    }

    /// <summary>
    /// 組み込みステージのみ取得
    /// </summary>
    public List<StageData> GetBuiltInStages()
    {
        return builtInStages.OrderBy(s => s.stageNumber).ToList();
    }

    /// <summary>
    /// ユーザーステージのみ取得
    /// </summary>
    public List<StageData> GetUserStages()
    {
        return userStages.OrderByDescending(s => s.createdAt).ToList();
    }

    /// <summary>
    /// IDでステージを取得
    /// </summary>
    public StageData GetStage(string id)
    {
        var stage = builtInStages.FirstOrDefault(s => s.id == id);
        if (stage != null) return stage;

        return userStages.FirstOrDefault(s => s.id == id);
    }

    /// <summary>
    /// 次のステージを取得（組み込みステージのみ）
    /// </summary>
    public StageData GetNextBuiltInStage(string currentId)
    {
        var current = builtInStages.FirstOrDefault(s => s.id == currentId);
        if (current == null) return null;

        return builtInStages
            .OrderBy(s => s.stageNumber)
            .FirstOrDefault(s => s.stageNumber > current.stageNumber);
    }

    /// <summary>
    /// ユーザーステージを保存（新規または更新）
    /// </summary>
    public void SaveUserStage(StageData stage)
    {
        stage.isBuiltIn = false;

        int index = userStages.FindIndex(s => s.id == stage.id);
        if (index >= 0)
        {
            userStages[index] = stage;
        }
        else
        {
            userStages.Add(stage);
        }

        SaveUserStages();
    }

    /// <summary>
    /// ユーザーステージを削除
    /// </summary>
    public void DeleteUserStage(string id)
    {
        userStages.RemoveAll(s => s.id == id);
        SaveUserStages();
    }

    /// <summary>
    /// 組み込みステージ数を取得
    /// </summary>
    public int BuiltInStageCount => builtInStages.Count;

    /// <summary>
    /// ユーザーステージ数を取得
    /// </summary>
    public int UserStageCount => userStages.Count;
}
