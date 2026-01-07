using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using MyGame;

/// <summary>
/// 存档管理器 - 处理游戏存档和读档
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("存档设置")]
    public string saveFolder = "Saves";
    public string saveExtension = ".sav";
    public int maxSaveSlots = 10;

    [Header("自动存档")]
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 300f;   // 5分钟
    private float autoSaveTimer = 0f;

    // 当前存档数据
    private SaveData currentSave;

    // 游戏开始时间（用于计算游戏时长）
    private float gameStartTime;

    // 存档路径
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFolder);

    // 事件
    public event Action<SaveData> OnSaveComplete;
    public event Action<SaveData> OnLoadComplete;
    public event Action<string> OnSaveError;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 确保存档文件夹存在
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        gameStartTime = Time.time;
    }

    private void Update()
    {
        // 自动存档
        if (autoSaveEnabled)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                AutoSave();
            }
        }
    }

    // ===== 存档 =====

    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slot, string saveName = null)
    {
        try
        {
            SaveData data = CollectSaveData();

            if (!string.IsNullOrEmpty(saveName))
            {
                data.saveName = saveName;
            }
            else
            {
                data.saveName = $"存档 {slot}";
            }

            string filePath = GetSaveFilePath(slot);
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(filePath, json);

            currentSave = data;
            OnSaveComplete?.Invoke(data);

            Debug.Log($"游戏已保存到槽位 {slot}: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"保存失败: {e.Message}");
            OnSaveError?.Invoke(e.Message);
            return false;
        }
    }

    /// <summary>
    /// 自动存档
    /// </summary>
    public void AutoSave()
    {
        SaveGame(0, "自动存档");
    }

    /// <summary>
    /// 快速存档
    /// </summary>
    public void QuickSave()
    {
        SaveGame(-1, "快速存档");
    }

    /// <summary>
    /// 收集存档数据
    /// </summary>
    private SaveData CollectSaveData()
    {
        SaveData data = new SaveData();

        // 存档信息
        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.playTimeSeconds = Mathf.RoundToInt(Time.time - gameStartTime);

        // 当前场景 - 使用完全限定名避免命名冲突
        data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 玩家数据
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            data.playerData = CollectPlayerData(player);
            data.playerPosition = player.transform.position;
            data.playerRotation = player.transform.eulerAngles;
        }

        // 进度数据
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null)
        {
            data.progressData = CollectProgressData(progress);
        }

        // 背包数据
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            data.inventoryData = CollectInventoryData(weaponManager, player);
        }

        // 迷雾数据
        FogOfWar fog = FogOfWar.Instance;
        if (fog != null)
        {
            data.fogData.sceneName = data.currentScene;
            data.fogData.exploredData = fog.GetExploredData();
        }

        return data;
    }

    /// <summary>
    /// 收集玩家数据
    /// </summary>
    private PlayerSaveData CollectPlayerData(Player player)
    {
        PlayerSaveData data = new PlayerSaveData();

        data.playerName = player.Name;
        data.currentHP = player.currentHP;

        if (player.combatData != null)
        {
            data.maxHP = player.combatData.maxHP;
            data.strength = player.combatData.strength;
            data.agility = player.combatData.agility;
            data.intelligence = player.combatData.intelligence;
            data.mobility = player.combatData.mobility;
            data.baseAC = player.combatData.baseAC;
        }

        // 从 GameProgressManager 获取等级信息
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null)
        {
            data.level = progress.playerLevel;
            data.currentExp = progress.currentExp;
            data.expToNextLevel = progress.expToNextLevel;
        }

        // 专长（如果有的话）
        // data.feats = player.GetFeats();

        return data;
    }

    /// <summary>
    /// 收集进度数据
    /// </summary>
    private ProgressSaveData CollectProgressData(GameProgressManager progress)
    {
        ProgressSaveData data = new ProgressSaveData();

        data.currentChapter = progress.currentChapter;
        data.currentStage = progress.currentStage;

        return data;
    }

    /// <summary>
    /// 收集背包数据
    /// </summary>
    private InventorySaveData CollectInventoryData(WeaponManager weaponManager, Player player)
    {
        InventorySaveData data = new InventorySaveData();

        // 使用反射获取武器列表
        var field = weaponManager.GetType().GetField("inventory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            var weapons = field.GetValue(weaponManager) as List<Weapon>;
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    Weapon w = weapons[i];
                    WeaponSaveData weaponData = new WeaponSaveData();
                    weaponData.weaponName = w.Name;

                    if (w is RangedWeapon ranged)
                    {
                        weaponData.weaponType = "Ranged";
                        weaponData.currentAmmo = ranged.CurrentAmmo;
                        weaponData.reserveAmmo = ranged.ReserveAmmo;
                    }
                    else if (w is BluntWeapon)
                    {
                        weaponData.weaponType = "Blunt";
                    }
                    else if (w is SharpWeapon)
                    {
                        weaponData.weaponType = "Sharp";
                    }

                    data.weapons.Add(weaponData);

                    // 检查是否是当前武器
                    if (player != null && player.currentWeapon == w)
                    {
                        data.currentWeaponIndex = i;
                    }
                }
            }
        }

        return data;
    }

    // ===== 读档 =====

    /// <summary>
    /// 从指定槽位加载游戏
    /// </summary>
    public bool LoadGame(int slot)
    {
        try
        {
            string filePath = GetSaveFilePath(slot);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"存档不存在: {filePath}");
                return false;
            }

            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("存档数据解析失败");
                return false;
            }

            currentSave = data;

            // 加载场景 - 使用完全限定名避免命名冲突
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedForSave;
            UnityEngine.SceneManagement.SceneManager.LoadScene(data.currentScene);

            Debug.Log($"正在加载存档 {slot}...");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载失败: {e.Message}");
            OnSaveError?.Invoke(e.Message);
            return false;
        }
    }

    /// <summary>
    /// 快速读档
    /// </summary>
    public bool QuickLoad()
    {
        return LoadGame(-1);
    }

    /// <summary>
    /// 场景加载完成后应用存档数据
    /// </summary>
    private void OnSceneLoadedForSave(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedForSave;

        // 延迟一帧应用数据，确保所有对象都已初始化
        StartCoroutine(ApplySaveDataDelayed());
    }

    private System.Collections.IEnumerator ApplySaveDataDelayed()
    {
        yield return null; // 等待一帧

        ApplySaveData(currentSave);
        OnLoadComplete?.Invoke(currentSave);

        Debug.Log("存档加载完成");
    }

    /// <summary>
    /// 应用存档数据
    /// </summary>
    private void ApplySaveData(SaveData data)
    {
        // 玩家数据
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            ApplyPlayerData(player, data.playerData);
            player.transform.position = data.playerPosition;
            player.transform.eulerAngles = data.playerRotation;
        }

        // 进度数据
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null)
        {
            ApplyProgressData(progress, data.progressData);
        }

        // 背包数据
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null && player != null)
        {
            ApplyInventoryData(weaponManager, player, data.inventoryData);
        }

        // 迷雾数据
        FogOfWar fog = FogOfWar.Instance;
        if (fog != null && data.fogData != null && data.fogData.sceneName == data.currentScene)
        {
            fog.LoadExploredData(data.fogData.exploredData);
        }

        // 更新游戏开始时间
        gameStartTime = Time.time - data.playTimeSeconds;
    }

    /// <summary>
    /// 应用玩家数据
    /// </summary>
    private void ApplyPlayerData(Player player, PlayerSaveData data)
    {
        // 通过 combatData 来设置 HP（因为 player.currentHP 是只读属性）
        if (player.combatData != null)
        {
            player.combatData.currentHP = data.currentHP;
            player.combatData.maxHP = data.maxHP;
            player.combatData.strength = data.strength;
            player.combatData.agility = data.agility;
            player.combatData.intelligence = data.intelligence;
            player.combatData.mobility = data.mobility;
            player.combatData.baseAC = data.baseAC;
        }

        // 等级信息
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null)
        {
            progress.playerLevel = data.level;
            progress.currentExp = data.currentExp;
            progress.expToNextLevel = data.expToNextLevel;
        }
    }

    /// <summary>
    /// 应用进度数据
    /// </summary>
    private void ApplyProgressData(GameProgressManager progress, ProgressSaveData data)
    {
        progress.currentChapter = data.currentChapter;
        progress.currentStage = data.currentStage;
    }

    /// <summary>
    /// 应用背包数据
    /// </summary>
    private void ApplyInventoryData(WeaponManager weaponManager, Player player, InventorySaveData data)
    {
        // 清空现有武器
        // weaponManager.ClearInventory();

        // 重建武器
        foreach (var weaponData in data.weapons)
        {
            Weapon weapon = CreateWeaponFromData(weaponData);
            if (weapon != null)
            {
                // weaponManager.AddWeapon(weapon);
            }
        }

        // 设置当前武器
        if (data.currentWeaponIndex >= 0 && data.currentWeaponIndex < data.weapons.Count)
        {
            // player.EquipWeapon(weapons[data.currentWeaponIndex]);
        }
    }

    /// <summary>
    /// 从存档数据创建武器
    /// </summary>
    private Weapon CreateWeaponFromData(WeaponSaveData data)
    {
        // 根据武器名称创建对应的武器
        // 这里需要根据你的武器工厂方法来实现

        switch (data.weaponName)
        {
            case "手枪":
                var pistol = RangedWeapon.CreatePistol();
                if (pistol is RangedWeapon ranged)
                {
                    ranged.CurrentAmmo = data.currentAmmo;
                    ranged.ReserveAmmo = data.reserveAmmo;
                }
                return pistol;

            case "棒球棍":
                return BluntWeapon.CreateBaseballBat();

            // 添加更多武器...

            default:
                Debug.LogWarning($"未知武器: {data.weaponName}");
                return null;
        }
    }

    // ===== 存档管理 =====

    /// <summary>
    /// 获取存档文件路径
    /// </summary>
    private string GetSaveFilePath(int slot)
    {
        string fileName = slot == -1 ? "quicksave" : $"save_{slot}";
        return Path.Combine(SavePath, fileName + saveExtension);
    }

    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists(int slot)
    {
        return File.Exists(GetSaveFilePath(slot));
    }

    /// <summary>
    /// 获取存档信息
    /// </summary>
    public SaveData GetSaveInfo(int slot)
    {
        try
        {
            string filePath = GetSaveFilePath(slot);
            if (!File.Exists(filePath)) return null;

            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取所有存档信息
    /// </summary>
    public List<SaveData> GetAllSaveInfos()
    {
        List<SaveData> saves = new List<SaveData>();

        // 快速存档
        SaveData quickSave = GetSaveInfo(-1);
        if (quickSave != null)
        {
            quickSave.saveName = "快速存档";
            saves.Add(quickSave);
        }

        // 自动存档
        SaveData autoSave = GetSaveInfo(0);
        if (autoSave != null)
        {
            autoSave.saveName = "自动存档";
            saves.Add(autoSave);
        }

        // 普通存档
        for (int i = 1; i <= maxSaveSlots; i++)
        {
            SaveData save = GetSaveInfo(i);
            if (save != null)
            {
                saves.Add(save);
            }
        }

        return saves;
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public bool DeleteSave(int slot)
    {
        try
        {
            string filePath = GetSaveFilePath(slot);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"已删除存档 {slot}");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"删除存档失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 格式化游戏时长
    /// </summary>
    public static string FormatPlayTime(int seconds)
    {
        int hours = seconds / 3600;
        int minutes = (seconds % 3600) / 60;
        int secs = seconds % 60;

        if (hours > 0)
        {
            return $"{hours}:{minutes:D2}:{secs:D2}";
        }
        else
        {
            return $"{minutes}:{secs:D2}";
        }
    }
}
