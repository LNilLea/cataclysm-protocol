using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏存档数据
/// </summary>
[Serializable]
public class SaveData
{
    // 存档信息
    public string saveName;
    public string saveTime;
    public int playTimeSeconds;

    // 玩家数据
    public PlayerSaveData playerData;

    // 进度数据
    public ProgressSaveData progressData;

    // 背包数据
    public InventorySaveData inventoryData;

    // 迷雾数据
    public FogSaveData fogData;

    // 场景数据
    public string currentScene;
    public Vector3Serializable playerPosition;
    public Vector3Serializable playerRotation;

    public SaveData()
    {
        saveName = "Save";
        saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        playTimeSeconds = 0;
        playerData = new PlayerSaveData();
        progressData = new ProgressSaveData();
        inventoryData = new InventorySaveData();
        fogData = new FogSaveData();
    }
}

/// <summary>
/// 玩家存档数据
/// </summary>
[Serializable]
public class PlayerSaveData
{
    // 基础属性
    public string playerName;
    public int level;
    public int currentExp;
    public int expToNextLevel;

    // 战斗属性
    public int maxHP;
    public int currentHP;
    public int strength;        // 体魄
    public int agility;         // 反应
    public int intelligence;    // 意志
    public int mobility;        // 移动力
    public int baseAC;

    // 专长
    public List<string> feats;

    public PlayerSaveData()
    {
        playerName = "Player";
        level = 1;
        currentExp = 0;
        expToNextLevel = 100;
        maxHP = 100;
        currentHP = 100;
        strength = 3;
        agility = 3;
        intelligence = 3;
        mobility = 3;
        baseAC = 10;
        feats = new List<string>();
    }
}

/// <summary>
/// 进度存档数据
/// </summary>
[Serializable]
public class ProgressSaveData
{
    public int currentChapter;
    public int currentStage;
    public List<string> completedRooms;
    public List<string> completedBattles;
    public List<string> unlockedAchievements;

    public ProgressSaveData()
    {
        currentChapter = 1;
        currentStage = 1;
        completedRooms = new List<string>();
        completedBattles = new List<string>();
        unlockedAchievements = new List<string>();
    }
}

/// <summary>
/// 背包存档数据
/// </summary>
[Serializable]
public class InventorySaveData
{
    public List<WeaponSaveData> weapons;
    public int currentWeaponIndex;
    public List<ItemSaveData> items;

    public InventorySaveData()
    {
        weapons = new List<WeaponSaveData>();
        currentWeaponIndex = -1;
        items = new List<ItemSaveData>();
    }
}

/// <summary>
/// 武器存档数据
/// </summary>
[Serializable]
public class WeaponSaveData
{
    public string weaponName;
    public string weaponType;       // "Blunt", "Sharp", "Ranged"
    public int currentAmmo;         // 枪械专用
    public int reserveAmmo;         // 枪械专用

    public WeaponSaveData()
    {
        weaponName = "";
        weaponType = "";
        currentAmmo = 0;
        reserveAmmo = 0;
    }
}

/// <summary>
/// 物品存档数据
/// </summary>
[Serializable]
public class ItemSaveData
{
    public string itemName;
    public string itemType;
    public int quantity;

    public ItemSaveData()
    {
        itemName = "";
        itemType = "";
        quantity = 0;
    }
}

/// <summary>
/// 迷雾存档数据
/// </summary>
[Serializable]
public class FogSaveData
{
    public string sceneName;
    public byte[] exploredData;

    public FogSaveData()
    {
        sceneName = "";
        exploredData = null;
    }
}

/// <summary>
/// 可序列化的 Vector3
/// </summary>
[Serializable]
public struct Vector3Serializable
{
    public float x;
    public float y;
    public float z;

    public Vector3Serializable(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static implicit operator Vector3(Vector3Serializable v)
    {
        return v.ToVector3();
    }

    public static implicit operator Vector3Serializable(Vector3 v)
    {
        return new Vector3Serializable(v);
    }
}
